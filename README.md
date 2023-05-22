# Web Api For Collecting RPG
수집형 RPG를 위한 Web API 서버

---

## 프로젝트 개발
[최다솜](https://github.com/kkumta)

---

## Tech Stack
<img src="https://img.shields.io/badge/C%23-512BD4?style=flat-square"/></a>
</br>
<img src="https://img.shields.io/badge/-512BD4?style=flat-square&logo=.NET"/></a>
</br>
<img src="https://img.shields.io/badge/MySQL-4479A1?style=flat-square"/></a>
<img src="https://img.shields.io/badge/SqlKata-E33D3A?style=flat-square"/></a>

---

## API 소개

### 계정 생성
```
﻿POST http://localhost:11500/api/createAccount
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "Password": "example password"
}
```

### 로그인
```
POST http://localhost:11500/api/login
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "Password": "example password"
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
}
```

### 공지 열람
```
POST http://localhost:11500/api/receiveNotice
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
}
```

### 우편 목록 열람
```
POST http://localhost:11500/api/getMails
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "Page": 열람을 원하는 페이지의 번호
}
```

### 개별 우편 열람
```
POST http://localhost:11500/api/getMail
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "MailId": 우편 ID
}
```

### 우편 아이템 수령
```
POST http://localhost:11500/api/receiveMailItems
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "MailId": 우편 ID
}
```

### 출석부 현황 열람
```
POST http://localhost:11500/api/getAttendance
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
}
```

### 출석 체크
```
POST http://localhost:11500/api/checkAttendance
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
}
```

### 인앱 상품 수령
```
POST http://localhost:11500/api/receiveInAppProduct
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "ReceiptInfo": {
    "ReceiptId": 영수증 ID,
    "ProductId": 인앱 상품 ID
  }
}
```

### 장비 강화
```
POST http://localhost:11500/api/enhanceItem
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "PlayerItemId": 플레이어-아이템 ID
}
```

### 던전 클리어 현황 열람
```
POST http://localhost:11500/api/getAllStages
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
}
```

### 스테이지 입장
```
POST http://localhost:11500/api/enterStage
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "StageId": 스테이지 ID
}
```

### 전투 중 아이템 파밍
```
POST http://localhost:11500/api/itemFarming
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "StageId": 스테이지 ID
  "ItemId": 아이템 ID
}
```

### 전투 중 적 NPC 처치
```
POST http://localhost:11500/api/killNpc
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "StageId": 스테이지 ID
  "NpcId": 적 NPC ID
}
```

### 스테이지 클리어
```
POST http://localhost:11500/api/completeStage
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "StageId": 스테이지 ID
}
```

### 명시적 전투 중단
```
POST http://localhost:11500/api/stopStage
Content-Type: application/json

{
  "Email": "example@gmail.com",
  "AuthToken": "로그인 시 발급받은 토큰",
  "ClientVersion": "클라이언트 버전",
  "MasterDataVersion": "마스터 데이터 버전"
  "StageId": 스테이지 ID
}
```

