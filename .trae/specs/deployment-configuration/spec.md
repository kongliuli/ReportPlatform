# 杏林 Web 报告编辑器 - 部署配置 PRD

## Overview
- **Summary**: 实现完整的部署配置，包括路由懒加载、模板缓存优化、画布渲染优化、Docker 容器化和 Nginx 反向代理配置。
- **Purpose**: 使系统能够高效地部署到生产环境，提供更好的性能和用户体验。
- **Target Users**: 运维人员、开发团队、最终用户

## Goals
- 实现路由懒加载，减少首屏加载时间
- 实现模板缓存机制，提升编辑体验
- 优化画布渲染性能，支持大模板编辑
- 提供完整的 Docker 容器化部署方案
- 配置 Nginx 反向代理和 WebSocket 支持

## Non-Goals (Out of Scope)
- Kubernetes 集群部署配置
- CI/CD 流水线配置
- 云服务商特定配置（如 AWS、Azure）
- HTTPS/SSL 证书配置（应由运维团队配置）

## Background & Context
- 当前项目已完成核心功能（认证、版本管理、编辑器 UI、数据绑定、PDF 预览）
- Phase 15 是最后一个阶段，主要关注性能优化和部署配置
- 项目采用前后端分离架构，需要分别配置

## Functional Requirements
- **FR-1**: 路由懒加载 - 实现动态 import 和 prefetch 优化
- **FR-2**: 模板缓存 - 内存缓存最近 10 个模板 + localStorage 缓存列表
- **FR-3**: 画布渲染优化 - 批量添加、对象缓存、rAF 节流
- **FR-4**: 后端 Dockerfile - 多阶段构建优化
- **FR-5**: 前端 Dockerfile - Node 构建 + Nginx 运行
- **FR-6**: Nginx 配置 - 反向代理、WebSocket、gzip 压缩
- **FR-7**: 生产配置 - SQL Server + 关闭 Swagger

## Non-Functional Requirements
- **NFR-1**: 首页 Lighthouse 评分 > 90
- **NFR-2**: 编辑器加载 100 元素 < 3s
- **NFR-3**: 拖拽帧率 > 30fps
- **NFR-4**: Docker 镜像体积最小化

## Constraints
- **Technical**: 使用 Vue 3 + .NET 8 技术栈，Docker 20.10+
- **Business**: 需要支持多种环境部署（开发、测试、生产）
- **Dependencies**: 需要 Docker 和 Docker Compose 环境

## Assumptions
- 运维团队负责配置 SSL 证书和域名
- 生产环境使用 SQL Server，开发环境使用 SQLite
- Docker Compose 用于本地开发和测试环境

## Acceptance Criteria

### AC-1: 路由懒加载实现
- **Given**: 用户访问应用首页
- **When**: 页面加载时
- **Then**: 只有首页相关代码被加载，其他路由组件按需加载
- **Verification**: `programmatic`
- **Notes**: 使用 Vue Router 的 defineAsyncComponent

### AC-2: 模板缓存机制
- **Given**: 用户编辑多个模板
- **When**: 用户切换模板或重新打开编辑器
- **Then**: 最近使用的模板从缓存中快速加载
- **Verification**: `programmatic`

### AC-3: 画布渲染性能
- **Given**: 编辑器加载包含 100 个元素的大模板
- **When**: 用户进行拖拽操作
- **Then**: 帧率保持在 30fps 以上
- **Verification**: `programmatic`

### AC-4: Docker 容器化
- **Given**: 运行 docker-compose build && docker-compose up
- **When**: 容器启动后
- **Then**: 前后端服务正常运行，API 可访问
- **Verification**: `programmatic`

### AC-5: Nginx 反向代理
- **Given**: 用户访问前端页面
- **When**: 发送 API 请求或 WebSocket 连接
- **Then**: 请求正确路由到后端服务
- **Verification**: `programmatic`

## Open Questions
- [ ] 是否需要配置健康检查端点？
- [ ] 是否需要配置日志收集？