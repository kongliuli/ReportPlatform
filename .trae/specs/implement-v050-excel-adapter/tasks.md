# Tasks

- [ ] Task 1: 创建 Excel 适配器模型和扁平化服务
  - [ ] 1.1: 创建 Services/ExcelAdapter/TemplateFieldSchema.cs（FlatField + TemplateFieldSchema）
  - [ ] 1.2: 创建 Services/ExcelAdapter/TemplateFlattenService.cs（Flatten + FlattenElement + FlattenTable + AddField）

- [ ] Task 2: 创建 Excel 契约导出服务
  - [ ] 2.1: 创建 Services/ExcelAdapter/ExcelSchemaExporter.cs（ExportTemplate + AddInstructionSheet）

- [ ] Task 3: 创建 Excel 契约读取服务
  - [ ] 3.1: 创建 Services/ExcelAdapter/ExcelContractReader.cs（ReadByContract + ReadBatchByContract + ReadContractRow）

- [ ] Task 4: 创建数据校验服务
  - [ ] 4.1: 创建 Services/ExcelAdapter/ExcelDataValidator.cs（Validate + ValidateNumber/Date/Dropdown/Boolean + ValidationReport + ValidationError）

- [ ] Task 5: 创建 Excel 适配器配置和工厂
  - [ ] 5.1: 创建 Services/ExcelAdapter/ExcelAdapterConfig.cs（ExcelAdapterConfig + ExcelTemplateSchema + ImportMode）
  - [ ] 5.2: 创建 Services/ExcelAdapter/ExcelAdapterFactory.cs（FlattenTemplate + ExportTemplate + ReadData + ReadBatchData + Validate）

- [ ] Task 6: 创建 ExcelAdapterTab ViewModel
  - [ ] 6.1: 创建 ViewModels/Tabs/ExcelAdapterTabViewModel.cs（Export/Import/Validate/BrowseFile 命令）

- [ ] Task 7: 创建 ExcelAdapterTab View
  - [ ] 7.1: 创建 Views/Tabs/ExcelAdapterTab.xaml（模板导出 + 数据导入 + 校验结果三区域）
  - [ ] 7.2: 创建 Views/Tabs/ExcelAdapterTab.xaml.cs

- [ ] Task 8: 集成到主界面
  - [ ] 8.1: 修改 App.xaml.cs 注册 ExcelAdapterFactory
  - [ ] 8.2: 修改 MainViewModel.cs 添加 Excel 适配器 Tab 创建逻辑
  - [ ] 8.3: 添加隐式 DataTemplate 映射 ExcelAdapterTabViewModel → ExcelAdapterTab

- [ ] Task 9: 验证
  - [ ] 9.1: 导出 xlsx 含隐藏契约行
  - [ ] 9.2: 按契约行精确导入
  - [ ] 9.3: 类型校验正常
  - [ ] 9.4: 批量导入正常

# Task Dependencies

- [Task 2] depends on [Task 1] (导出依赖扁平化结果)
- [Task 3] depends on [Task 1] (读取依赖 FlatField 定义)
- [Task 4] depends on [Task 1] (校验依赖 FlatField 定义)
- [Task 5] depends on [Task 1, Task 2, Task 3, Task 4] (工厂依赖所有组件)
- [Task 6] depends on [Task 5] (ViewModel 依赖工厂)
- [Task 7] depends on [Task 6] (View 依赖 ViewModel)
- [Task 8] depends on [Task 7] (集成依赖 View)
- [Task 9] depends on [Task 8]

# Parallelizable Work

- Task 2, Task 3, Task 4 可并行（均仅依赖 Task 1 的模型定义）
