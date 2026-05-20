## 2026-05-17 Learnings

### Environment Constraints
- No API keys configured (`DEEPSEEK_API_KEY` empty) — `task()` subagents cannot connect to any LLM backend
- `dotnet` CLI not available in WSL PATH — must use Windows `dotnet.exe` at `/mnt/c/Program Files/dotnet/dotnet.exe`
- Google Gemini models fail with "Google Generative AI API key is missing"
- Deepseek models fail with "\"undefined/chat/completions\" cannot be parsed as a URL" (base URL not configured)
- Task delegation via `task()` is non-functional — all implementation work must be done directly

### Task A - Dropdown Dedup
- `ContextAdapterTabViewModel.ExecuteAddFromUnconfigured` at line 230
- Added `UnconfiguredFields.Remove(SelectedUnconfiguredField)` and `SelectedUnconfiguredField = null` after `StaticValues.Add()`
- Preserved existing duplicate check (lines 234-240)

### Task B - Date/Gender Heuristic
- `MainTabViewModel.CreateFieldViewModel` — heuristic block before `return field`
- Guard: only triggers when `field.FieldType == FieldDataType.Text`
- "日期" or "date" (lower) → FieldDataType.Date
- "性别" → FieldDataType.Dropdown (with Options populated if available)
- NOTE: The condition `label.Contains("性别") || (element.Options?.Any() == true && label.Contains("性别"))` is redundant (A || (B && A) = A). Works correctly but could be simplified.

### Task C - FillContext Skip
- `ContextAdapterService.FillContext` line 33
- Changed from unconditional `data[...] = ResolveBuiltIn(...)` to conditional
- Only adds to data dict when `ResolveBuiltIn()` returns non-empty string

### Task D - Panel Split
- `MainTabViewModel.LoadFields` — removed `|| !string.IsNullOrEmpty(element.DefaultValue)` condition
- Updated section titles: "布局固定内容", "适配器已匹配/已配置"

### Task E - Scroll Propagation
- `MainTab.xaml` — added `x:Name="MainScrollViewer"` and `PreviewMouseWheel` handler
- `MainTab.xaml.cs` — added handler that scrolls ScrollViewer and sets `e.Handled = true`
- Added `using System.Windows.Input`

### Task F - Documentation
- Created `docs/element-type-contract.md` — 109 lines
- Type mapping table with 22 CLR types
- Heuristic fallback documentation
- Known issues section

### Build
- `dotnet build` passes with zero errors via Windows dotnet.exe
- Pre-existing warnings in Editor project (SKPaint deprecation) — not from our changes
