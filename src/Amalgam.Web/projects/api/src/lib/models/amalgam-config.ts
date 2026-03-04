import { BackendConfig } from './backend-config';
import { FrontendConfig } from './frontend-config';
import { RepositoryConfig } from './repository-config';

export interface AmalgamConfig {
  repositories: RepositoryConfig[];
  backend: BackendConfig;
  frontend: FrontendConfig;
}
