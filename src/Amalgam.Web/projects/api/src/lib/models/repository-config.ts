import { MergeConfig } from './merge-config';
import { RepositoryType } from './repository-type';

export interface RepositoryConfig {
  name: string;
  type: RepositoryType;
  path: string;
  enabled: boolean;
  routePrefix?: string;
  packageName?: string;
  merge?: MergeConfig;
}
