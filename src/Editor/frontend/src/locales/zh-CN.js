// 中文文本配置
// 退化方案：不引入 vue-i18n，纯抽配置
// 使用方式：import $t from '@/locales/zh-CN'
// 在模板中：{{ $t('app.title') }} 或 v-text="$t('app.title')"

const messages = {
  // ====== 全局 ======
  app: {
    title: '报告模板编辑器',
    brand: '模板编辑器',
  },

  // ====== 导航/路由页面标题 ======
  pages: {
    home: '首页',
    templates: '模板管理',
    settings: '系统设置',
    editor: '模板编辑器',
    versions: '版本历史',
    defaultTitle: '报告模板编辑器',
  },

  // ====== 登录页 ======
  login: {
    title: '欢迎回来',
    subtitle: '登录以继续使用报告模板编辑器',
    username: '用户名',
    password: '密码',
    usernamePlaceholder: '请输入用户名',
    passwordPlaceholder: '请输入密码',
    loginButton: '登录',
    loggingIn: '登录中...',
    loginSuccess: '登录成功',
    loginFailed: '登录失败，请检查用户名和密码',
    usernameRequired: '请输入用户名',
    passwordRequired: '请输入密码',
    footer: '报告模板 Web 编辑器',
  },

  // ====== 功能特性（登录页） ======
  features: {
    heading: '功能特性',
    editor: '可视化编辑',
    editorDesc: '拖拽式模板设计，所见即所得',
    version: '版本管理',
    versionDesc: '自动保存历史版本，随时回溯',
    databinding: '数据绑定',
    databindingDesc: '灵活的数据源绑定与转换',
    export: '多格式导出',
    exportDesc: '支持 PDF、JSON 等格式导出',
  },

  // ====== 首页 ======
  home: {
    welcome: '欢迎回来',
    subtitle: '开始创建和管理您的报告模板',
    newTemplate: '新建模板',
    totalTemplates: '模板总数',
    published: '已发布',
    draft: '草稿',
    pending: '待处理',
    todayEdited: '今日编辑',
    today: '今日',
    active: '活跃',
    recentEdited: '最近编辑',
    viewAll: '查看全部',
    noTemplates: '暂无模板',
    createFirst: '创建第一个模板',
    quickActions: '快捷操作',
    templateList: '模板列表',
    searchTemplate: '搜索模板',
    systemSettings: '系统设置',
    tips: '使用提示',
    tip1: '使用拖拽方式在画布上添加元素',
    tip2: '双击元素可快速编辑内容',
    tip3: 'Ctrl+S 快速保存模板',
  },

  // ====== 模板管理 ======
  templates: {
    title: '模板管理',
    description: '创建、编辑和管理您的报告模板',
    newTemplate: '新建模板',
    search: '搜索模板名称...',
    allTypes: '全部类型',
    edit: '编辑',
    delete: '删除',
    deleteConfirm: '确定要删除此模板吗？此操作不可恢复。',
    deleteTitle: '删除确认',
    confirmDelete: '确定',
    cancel: '取消',
    createSuccess: '创建成功',
    updateSuccess: '更新成功',
    deleteSuccess: '删除成功',
  },

  // ====== 工具栏 ======
  toolbar: {
    back: '返回',
    zoomIn: '放大',
    zoomOut: '缩小',
    fitScreen: '适应屏幕',
    showGrid: '显示网格',
    hideGrid: '隐藏网格',
    pageSettings: '页面设置',
    exportJson: '导出JSON',
    preview: '预览',
    save: '保存',
  },

  // ====== 页面设置 ======
  pageSettings: {
    title: '页面设置',
    paperSize: '纸张大小',
    orientation: '纸张方向',
    portrait: '纵向',
    landscape: '横向',
    margins: '页边距(mm)',
    preview: '预览',
    cancel: '取消',
    apply: '应用',
    confirmMessage: '切换页面设置可能导致部分元素超出页面范围，是否继续？',
    confirmTitle: '页面设置变更',
    confirmButton: '继续',
    confirmCancel: '取消',
    custom: '自定义',
  },

  // ====== 属性面板 ======
  properties: {
    noSelection: '选择元素以编辑属性',
    noElements: '暂无元素',
    noTemplate: '请选择或创建模板',
    basicProps: '基础属性',
    appearance: '外观',
    font: '字体',
    databinding: '数据绑定',
    label: '标签',
    x: 'X (mm)',
    y: 'Y (mm)',
    width: '宽 (mm)',
    height: '高 (mm)',
    foregroundColor: '前景色',
    backgroundColor: '背景色',
    opacity: '透明度',
    borderColor: '边框色',
    borderWidth: '边框宽',
    cornerRadius: '圆角',
    fontFamily: '字体',
    fontWeight: '粗细',
    bold: '粗体',
    alignment: '对齐',
    dataPath: '数据路径',
    format: '格式化',
  },

  // ====== 数据绑定面板 ======
  databinding: {
    bindPath: '绑定路径',
    transformRules: '转换规则',
    validationRules: '验证规则',
  },

  // ====== 图层面板 ======
  layers: {
    title: '图层',
    empty: '暂无元素',
  },

  // ====== 工具盒 ======
  toolbox: {
    search: '搜索元素...',
  },

  // ====== 角色 ======
  roles: {
    admin: '管理员',
    editor: '编辑者',
    viewer: '查看者',
  },

  // ====== 用户菜单 ======
  userMenu: {
    profile: '个人资料',
    logout: '退出登录',
  },

  // ====== 侧边栏 ======
  sidebar: {
    expand: '展开侧边栏',
    collapse: '收起侧边栏',
    version: '版本',
  },

  // ====== 版本历史 ======
  versions: {
    title: '版本历史',
    // no other common strings identified yet
  },

  // ====== 编辑器 ======
  editor: {
    loadFailed: '加载模板失败',
    jsonExported: 'JSON 已导出',
    pageSettingsUpdated: '页面设置已更新',
    undoLabel: '撤销',
    redoLabel: '重做',
  },

  // ====== 错误/通用消息 ======
  error: {
    templateNotFound: '模板不存在',
    operationFailed: '操作失败',
    networkError: '网络错误',
  },
}

export default function $t(key) {
  const keys = key.split('.')
  let result = messages
  for (const k of keys) {
    result = result?.[k]
  }
  return result ?? key
}

export { messages }
