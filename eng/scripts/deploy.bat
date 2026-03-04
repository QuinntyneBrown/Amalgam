@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo Usage: deploy.bat ^<resource-group-name^>
    exit /b 1
)

set RESOURCE_GROUP=%~1

REM Navigate to repository root
pushd %~dp0..\..

REM Deploy infrastructure first to ensure ACR exists
echo Running Bicep deployment to create infrastructure...
az deployment group create -g %RESOURCE_GROUP% -n main --template-file infra/main.bicep --parameters infra/main.bicepparam

REM Get ACR login server from the deployment outputs
echo Retrieving ACR login server...
for /f "tokens=*" %%i in ('az deployment group show -g %RESOURCE_GROUP% -n main --query "properties.outputs.acrLoginServer.value" -o tsv') do set ACR_LOGIN_SERVER=%%i

echo ACR Login Server: %ACR_LOGIN_SERVER%

REM Log in to ACR
echo Logging in to ACR...
az acr login --name %ACR_LOGIN_SERVER%

REM Build and tag API image
echo Building API image...
docker build -t %ACR_LOGIN_SERVER%/amalgam-api:latest -f src/Amalgam.Api/Dockerfile .

REM Build and tag Web image
echo Building Web image...
docker build -t %ACR_LOGIN_SERVER%/amalgam-web:latest -f src/Amalgam.Web/Dockerfile src/Amalgam.Web

REM Push images to ACR
echo Pushing API image...
docker push %ACR_LOGIN_SERVER%/amalgam-api:latest

echo Pushing Web image...
docker push %ACR_LOGIN_SERVER%/amalgam-web:latest

REM Deploy with Bicep, passing container image overrides
echo Running Bicep deployment...
az deployment group create ^
    -g %RESOURCE_GROUP% ^
    --template-file infra/main.bicep ^
    --parameters infra/main.bicepparam ^
    --parameters apiContainerImage=%ACR_LOGIN_SERVER%/amalgam-api:latest ^
    --parameters webContainerImage=%ACR_LOGIN_SERVER%/amalgam-web:latest

echo.
echo Deployment complete!
echo Fetching app URLs...
az deployment group show -g %RESOURCE_GROUP% -n main --query "properties.outputs" -o table

popd
endlocal
