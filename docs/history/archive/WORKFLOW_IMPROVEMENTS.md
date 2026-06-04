# 工作流优化建议

> 基于 2026-05-10 当天的实际操作体验提炼。

---

## 1. 代理会话 (agent delegation) 优化

### 痛点
- 每次 `task()` 代理冷启动 ~15s（模型路由 + 上下文加载）
- `deepseek-chat` 用于 deep/ultrabrain 类别时失败率高
- 简单机械操作走 delegate 反而比直接做慢 5-10 倍

### 建议规则
```
简单操作（单文件改、读） → 直接读写，不 delegate
中等复杂度（多文件改） → 直接做 + build 验证
高复杂度（架构级/需要深度分析）→ delegate deep 类别
Delegate 超时 3 分钟 → 切回直接做
Delegate 失败 2 次 → 切回直接做
```

### 次优配置
- `quick` 类别应设置为 `deepseek-chat`（快速）
- `explore/librarian` 应设置为延迟最小模型
- 当前 `oh-my-openagent.json` 中 `deepseek-chat` 映射正确

---

## 2. Shell / Build 超时处理

### 痛点
- `dotnet build` 首次跑 NuGet restore 需 2-5 分钟
- 多次被系统 kill（超时 3 分钟）后重新跑，浪费累计时间
- 误用 `dotnet build` 去"检查"已有 build，重新编译而非读日志

### 建议规则
```
所有 shell 命令：
  1. 设超时阈值 3 分钟
  2. 超时未返回 → 视为后台运行，立即切到其他任务
  3. 每 2 分钟检查一次结果（不是重新跑命令）
  4. Build 产物检查：看 bin/Debug/ 目录是否存在 .dll
  5. 日志检查：看上次 build 的 stdout/stderr
```

### 实用检查方法
```powershell
# 不重新 build，检查上次结果
$dlls = Get-ChildItem "path/bin/Debug/net*/" -Filter "*.dll"
$dlls.Count -gt 0  # true = 上次 build 成功

# 检查 NuGet 包是否 restore
Test-Path "path/obj/project.assets.json"
```

---

## 3. 可见性 / 调度链路追踪

### 痛点
- `background_output(task_id)` 多次返回 "Task not found"
- Session 被快速清理导致代理产丢失
- 用户无法直观看到 agent → model → action 的映射

### 建议改进
- 为每个代理任务添加 `description` 字段明确标注成
- 代理产出丢失时切到直接验证文件存在性
- 在 `.sisyphus/` 中维护调度日志（可选）

---

## 4. 模型选择策略

### 痛点
所有模型通过 DeepSeek API 走，单链路：
- 无其他模型可 fallback
- `deepseek-chat` 在复杂任务上失败率高
- 中转站 sssaicode 一直 404

### 建议行动
1. 获取 OpenRouter API Key → 添加 Free 模型作为 fallback
2. 获取 Kimi API Key → 添加作为写作/前端类别
3. `oh-my-openagent.json` 配置 fallback chain：
   ```
   primary: deepseek-chat
   fallback_1: openrouter/llama-3.2-3b (免费)
   fallback_2: kimi/moonshot-v1-8k (需 Key)
   ```

---

## 5. 全局规则固化

### 痛点
行为规则写在会话上下文里，每次新会话不继承。

###当前状态
- 已创建 `~/.config/opencode/.sisyphus/SYSTEM_BEHAVIOR.md`（全局）
- 已创建项目级 `ReportPlatform/.sisyphus/SYSTEM_BEHAVIOR.md`（项目）
- `oh-my-openagent.json` 中 sisyphus comment 已标注规则路径

### 待完善
- oh-my-openagent 不支持自定义 system prompt，规则文件是"人在回路"模式
- 如果未来 oh-my-openagent 支持 `prompt` 字段，规则可直接注入

---

## 6. 文件读取策略

### 痛点
- `grep` 工具依赖 `rg`（ripgrep），Windows 未安装，反复失败
- 切换 `Select-String` 时参数差异导致报错

### 建议
```powershell
# 替代 grep 的固定写法：
Get-ChildItem -Recurse -Filter "*.cs" | Select-String "pattern" -SimpleMatch

# 不要用 rg/grep 工具报错后重试超过 2 次
# 直接切换到 PowerShell 原生方法
```

---

## 7. 明日开工流程

1. [ ] 检查 build 状态：`Get-ChildItem Editor/Server.Tests/bin/Debug/net8.0/*.dll`
2. [ ] 未通过 → 修 H 测试编译错误
3. [ ] 通过 → 跑测试 `dotnet test`
4. [ ] 推 P（设计令牌化）：扫描 components/ 中 scoped style 硬编码
5. [ ] 推 N（上下文适配器 Web API）：创建 ContextController
6. [ ] 推 O（i18n 抽配置）：提取中文到 locales/zh-CN.json
