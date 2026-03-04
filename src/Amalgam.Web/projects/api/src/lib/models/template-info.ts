import { AmalgamConfig } from './amalgam-config';
import { TemplateSummary } from './template-summary';

export interface TemplateInfo extends TemplateSummary {
  config: AmalgamConfig;
}
