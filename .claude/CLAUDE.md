# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture

.NET 8 Clean Architecture: **Core** (entities, interfaces) <- **Application** (services, DTOs) <- **Infrastructure** (EF Core, repos, security) <- **API** (controllers, DI)

## Skills

When implementing features, **read the relevant skill file first**:

| Task | Skill file |
|------|------------|
| Adding endpoint | `.claude/skills/new-endpoint.md` |
| Adding entity | `.claude/skills/new-entity.md` |
| Adding service | `.claude/skills/new-service.md` |
| Writing unit test | `.claude/skills/unit-test.md` |
| Writing integration test | `.claude/skills/integration-test.md` |

## Commands

| Command | Purpose |
|---------|---------|
| `/build` | Build solution |
| `/test [unit\|integration\|coverage\|name]` | Run tests |
| `/run` | Start API |
