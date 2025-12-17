# ASF-Bot

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/d9f65c78630449a185fb582e95a14a47)](https://www.codacy.com/gh/chr233/asf-bot/dashboard)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/chr233/asf-bot/publish.yml?logo=github)
[![License](https://img.shields.io/github/license/chr233/asf-bot?logo=apache)](https://github.com/chr233/asf-bot/blob/master/license)

![GitHub Repo stars](https://img.shields.io/github/stars/chr233/asf-bot?style=flat&logo=github)
![GitHub forks](https://img.shields.io/github/forks/chr233/asf-bot?style=flat&logo=github)
[![GitHub Download](https://img.shields.io/github/downloads/chr233/asf-bot/total?logo=github)](https://img.shields.io/github/v/release/chr233/asf-bot)

[![GitHub Release](https://img.shields.io/github/v/release/chr233/asf-bot?logo=github)](https://github.com/chr233/asf-bot/releases)
[![GitHub Release](https://img.shields.io/github/v/release/chr233/asf-bot?include_prereleases&label=pre-release&logo=github)](https://github.com/chr233/asf-bot/releases)
![GitHub last commit](https://img.shields.io/github/last-commit/chr233/asf-bot?logo=github)

[![爱发电](https://img.shields.io/badge/爱发电-chr__-ea4aaa.svg?logo=github-sponsors)](https://afdian.net/@chr233)

## 支持多 IPC 管理的 ASF 机器人

## 配置文件

> 配置文件位置 config/config.json
>
> 如果不需要文件日志, 可以删除 config/nlog.config

```json
{
  "System": {
    "Debug": false,
    "Statistic": true
  },

  "Database": {
    "Generate": true,
    "LogSQL": false,

    //使用 Sqlite
    "DbType": "sqlite",
    "DbName": "data"

    //使用 Mysql, 也支持 Pgsql
    //"DbType": "mysql",
    //"DbHost": "localhost",
    //"DbPort": 3306,
    //"DbName": "db_name",
    //"DbUser": "root",
    //"DbPassword": "root"
  },

  "Telegram": {
    "Proxy": null,
    "BotToken": "123456:abcdefg",
    "AdminUsers": [],
    "DropPendingUpdates": false
  },

  "AsfIpcs": [
    {
      "Name": "example",
      "BaseUrl": "http://127.0.0.1:1242",
      "IpcPassword": "",
      "Proxy": null
    }
  ]
}
```
