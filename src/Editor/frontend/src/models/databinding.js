export class TemplateDataBinding {
  constructor(props = {}) {
    this.id = props.id || crypto.randomUUID()
    this.elementId = props.elementId ?? ''
    this.dataPath = props.dataPath ?? ''
    this.formatString = props.formatString ?? ''
  }
}

export class DataBindingPaths {
  constructor(props = {}) {
    this.mainPath = props.mainPath ?? ''
    this.additionalPaths = props.additionalPaths ?? {}
  }
}

export class DataTransformRule {
  constructor(props = {}) {
    this.type = props.type ?? 'Format'
    this.params = props.params ?? {}
  }

  static TYPES = ['Format', 'Case', 'Math', 'DateFormat', 'Custom']
}

export class DataValidationRule {
  constructor(props = {}) {
    this.type = props.type ?? 'Required'
    this.params = props.params ?? {}
  }

  static TYPES = ['Required', 'Range', 'Regex', 'Length', 'Custom']
}
