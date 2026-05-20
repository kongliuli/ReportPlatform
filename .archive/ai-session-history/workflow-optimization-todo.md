# 工作流优化待完善操作

> 基于 2026-05-10 开发中的观察和用户主动提出的改进需求。

---

## 一、系统规则层面（已落地）

以下规则已在 `.sisyphus/SYSTEM_BEHAVIOR.md` 中定义（项目级 + 用户级）：

| 规则 | 内容 |
|------|------|
| 任务完成汇报 | 完成后检查无后台任务 → 汇总改动 → 列出下一步方向 → 等待指示 |
| Shell/Build 超时 | 超 3 分钟未返回 → 视为后台运行，切其他任务，每 2 分钟嗅探结果 |
| 执行原则 | 优先直接操作，仅复杂任务 delegate，机械重复自己做 |

**待改进**: 当前 system behavior 文件是纯文本 markdown，Sisyphus 不会自动读取。每次会话开始时需要手动确认规则。

---

## 二、代理会话效率（核心痛点）

### 观察到的问题

| 问题 | 根因 | 影响 |
|------|------|------|
| delegate 任务 7 分钟才完成 | DeepSeek API 推理速度 + 会话冷启动(~15s) + 工具调用延迟 | 小改动不如自己做 |
| 23 个 Renderer 更新只完成 1/23 | 代理 session 超时/断连 | 被迫重跑 |
| `background_output` 返回 "not found" | 会话被快速清理 | 无法追踪产出 |
| explore/librarian agent 慢 | 同样的冷启动开销，但只是做 grep | 直接 grep 更快 |

### 优化方向（待决策）

| # | 优化 | 效果 | 代价 |
|---|------|------|------|
| 1 | **机械操作不做 delegate**，直接读/写 | 节省 80% 代理开销 | 无 |
| 2 | delegate 前评估：工具调用轮次 > 3？→ 自己做 | 避免长尾 task | 需要预判 |
| 3 | 换用更快模型做 quick（如 gpt-5.4-mini） | 推理速度可能提升 | 需要中转站可用 |
| 4 | 改进会话清理策略，保留产出日志 | 可追踪 agent 结果 | 需 oh-my-openagent 支持 |

---

## 三、中转站依赖问题

### 当前状态

- DeepSeek 直连可用（但推理慢）
- sssaicode 中转站 404（已从配置删除）
- OpenRouter/Kimi 模型已添加模板，**无 API Key**

### 依赖关系

```
Sisyphus/Orchestrator → DeepSeek Chat (当前主力)
                    ↓ 中等级
oracle/ultrabrain   → DeepSeek Reasoner (慢，但思考深)
                    ↓ 低等级
explore/librarian   → DeepSeek Chat (只用它做简单搜索，浪费)
                    ↓ 未使用
Claude Opus/Sonnet → 需中转站修复
GPT-5.x 系列       → 需中转站修复
```

### 待决策

- 是否补齐中转站 API Key？或找其他可用中转站？
- 是否直接申请 OpenRouter Key 用免费模型？
- gpt-5.4-mini 如果可用，能显著提升 quick agent 速度

---

## 四、开发流程优化

### 4.1 测试策略

| 当前 | 改进方向 |
|------|----------|
| Playwright E2E 8 个测试通过 | 建立 CI 触发器，每次改完前端自动跑 |
| API 测试 30 个未编译通过 | 修复 DTO 属性后，加入 dotnet test 自动化 |
| 无 unit test | 关键 Service 层（Auth/Template/Version）加单元测试 |

### 4.2 文件改动追踪

| 当前 | 改进方向 |
|------|----------|
| 无改动记录 | 每个 Phase 的改动在 `daily-summary.md` 中维护 |
| 无从知道谁改了啥 | agent 调度日志（oh-my-openagent 层面暂不支持） |

### 4.3 任务粒度控制

| 当前 | 改进方向 |
|------|----------|
| 一个小改动也 delegate 7 分钟 | < 3 个文件的改动：自己直接做 |
| 机械重复 23 个文件也 delegate | 批量编辑走脚本或 iterate，不做 agent |

---

## 五、代码库已知待优化（明日可直接开干）

### P0（快速出活）

| 任务 | 文件 | 操作 | 预估 |
|------|------|------|------|
| API 测试修复 | 5 个 Tests 文件 | 改 DTO 属性名 + Assert 条件 | 15min |
| CSS 硬编码归一化 | 扫描 Vue scoped style | 将 `#e8e8e8` 等硬编码值改为 `var(--color-xxx)` | 20min |

### P1（中等价值）

| 任务 | 文件 | 操作 | 预估 |
|------|------|------|------|
| 前端硬编码中文抽离 | 10+ Vue 组件 | 创建 `locales/zh-CN.js`，替换 template 中的中文文本 | 30min |
| PDF 渲染器补全 | `PdfTemplateRenderer.cs` | Barcode/QrCode/Chart/Container/Repeat/Hyperlink/Icon/Signature/Watermark | 20min |

### P2（长期）

| 任务 | 操作 | 预估 |
|------|------|------|
| Docker 部署验证 | `docker-compose up` + 验证 | 30min |
| 上下文适配器 Web API 暴露 | 将 ContextAdapterService 桥接到 Editor.Server | 1h |
