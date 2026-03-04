import { ValidationResult } from './validation-result';

export interface DashboardSummary {
  totalRepositories: number;
  countByType: Record<string, number>;
  validation: ValidationResult;
}
