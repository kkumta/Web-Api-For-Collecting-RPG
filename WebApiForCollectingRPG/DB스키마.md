# AccountDB
  
## Account Table
게임에서 생성된 계정 정보를 가지고 있는 테이블    
  
```sql
DROP DATABASE IF EXISTS AccountDB;
CREATE DATABASE IF NOT EXISTS AccountDB;

USE AccountDB;

DROP TABLE IF EXISTS AccountDB.`Account`;
CREATE TABLE IF NOT EXISTS AccountDB.`Account`
(
    AccountId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정 번호',
    Email VARCHAR(50) NOT NULL UNIQUE COMMENT '이메일',
    SaltValue VARCHAR(100) NOT NULL COMMENT  '암호화 값',
    HashedPassword VARCHAR(100) NOT NULL COMMENT '해싱된 비밀번호',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '생성 날짜'
) COMMENT '계정 정보 테이블';
```
   
<br>  
<br>  

# MasterDB

```sql
CREATE DATABASE IF NOT EXISTS MasterDB;
```
  
## Item Table
아이템 정보

```sql

USE MasterDB;
DROP TABLE IF EXISTS MasterDB.`Item`;
CREATE TABLE IF NOT EXISTS MasterDB.`Item`
(
    ItemId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
    Name VARCHAR(50) UNIQUE NOT NULL COMMENT '아이템 이름',
    AttributeId INT COMMENT  '아이템 특성',
    SellPrice BIGINT COMMENT  '파는 가격',
    BuyPrice BIGINT COMMENT '사는 가격',
    UseLv SMALLINT COMMENT '사용 가능 레벨',
    Attack BIGINT COMMENT '공격력',
    Defence BIGINT  COMMENT '방어력',
    Magic BIGINT COMMENT '마력',
    EnhanceMaxCount SMALLINT COMMENT '최대 강화 횟수', 
    IsItemStackable BOOL COMMENT '아이템 겹침 보관 가능 여부'

) COMMENT '아이템 정보';

insert into Item (Name, Attribute, SellPrice, BuyPrice, UseLv, Attack, Defence, Magic, EnhanceMaxCount, IsItemStackable) values
("돈", 5, 0, 0, 0, 0, 0, 0, 0, true),
("작은 칼", 1, 10, 20, 1, 10, 5, 1, 10, false),
("도금 칼", 1, 100, 200, 5, 29, 12, 10, 10, false),
("나무 방패", 2, 7, 15, 1, 3, 10, 1, 10, false),
("보통 모자", 3, 5, 8, 1, 1, 1, 1, 10, false),
("포션", 4, 3, 6, 1, 0, 0, 0, 0, true);
```

## ItemAttribute Table
아이템 특성 정보

```sql
USE MasterDB;
DROP TABLE IF EXISTS MasterDB.`ItemAttribute`;
CREATE TABLE IF NOT EXISTS MasterDB.`ItemAttribute`
(
    AttributeId INT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '특성 번호',
    Name VARCHAR(50) UNIQUE NOT NULL COMMENT '특성 이름'
) COMMENT '아이템 특성 정보';

insert into ItemAttribute (Name) values
("무기"),
("방어구"),
("복장"),
("마법도구"),
("돈");
```

## AttendanceCompensation Table
출석부 보상

```sql
USE MasterDB;
DROP TABLE IF EXISTS MasterDB.`AttendanceCompensation`;
CREATE TABLE IF NOT EXISTS MasterDB.`AttendanceCompensation`
(
    CompensationId SMALLINT NOT NULL PRIMARY KEY COMMENT '보상 번호',
    ItemId BIGINT NOT NULL COMMENT '아이템 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수'
) COMMENT '출석부 보상';

insert into AttendanceCompensation (CompensationId, ItemId, ItemCount) values
(1, 1, 100),
(2, 1, 100),
(3, 1, 100),
(4, 1, 200),
(5, 1, 200),
(6, 1, 200),
(7, 2, 1),
(8, 1, 100),
(9, 1, 100),
(10, 1, 100),
(11, 6, 5),
(12, 1, 150),
(13, 1, 150),
(14, 1, 150),
(15, 1, 150),
(16, 1, 150),
(17, 1, 150),
(18, 4, 1),
(19, 1, 200),
(20, 1, 200),
(21, 1, 200),
(22, 1, 200),
(23, 1, 200),
(24, 5, 1),
(25, 1, 250),
(26, 1, 250),
(27, 1, 250),
(28, 1, 250),
(29, 1, 250),
(30, 3, 1);
```

## InAppProduct Table
인앱 상품

```sql
USE MasterDB;
DROP TABLE IF EXISTS MasterDB.`InAppProduct`;
CREATE TABLE IF NOT EXISTS MasterDB.`InAppProduct`
(
    ProductId SMALLINT NOT NULL COMMENT '상품 번호',
    ItemId BIGINT NOT NULL COMMENT '아이템 번호',
    ItemName VARCHAR(50) NOT NULL COMMENT '아이템 이름',
    ItemCount INT NOT NULL COMMENT '아이템 개수'
) COMMENT '인앱 상품 정보';

insert into InAppProduct (ProductId, ItemId, ItemName, ItemCount) values
(1, 1, "돈", 1000),
(1, 2, "작은 칼", 1),
(1, 3, "도금 칼", 1),
(2, 4, "나무 방패", 1),
(2, 5, "보통 모자", 1),
(2, 6, "포션", 10),
(3, 1, "돈", 2000),
(3, 2, "작은 칼", 1),
(3, 3, "나무 방패", 1),
(3, 5, "보통 모자", 1);
```


# GameDB

```sql
DROP DATABASE IF EXISTS GameDB;
CREATE DATABASE IF NOT EXISTS GameDB;
```
  
## AccountGame Table
각 유저별 게임 데이터

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`AccountGame`;
CREATE TABLE IF NOT EXISTS GameDB.`AccountGame`
(
    AccountId BIGINT NOT NULL PRIMARY KEY COMMENT '계정 번호',
    Money BIGINT NOT NULL COMMENT '돈',
    Exp BIGINT NOT NULL COMMENT '경험치'
) COMMENT '유저별 게임 데이터';
```
  
## AccountItem Table
각 유저별 아이템 데이터

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`AccountItem`;
CREATE TABLE IF NOT EXISTS GameDB.`AccountItem`
(
    AccountItemId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정-아이템 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    ItemId BIGINT NOT NULL COMMENT '아이템 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수',
    EnhanceCount SMALLINT NOT NULL COMMENT '강화횟수'
) COMMENT '유저별 아이템 데이터';
```

## Mail Table
우편

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`Mail`;
CREATE TABLE IF NOT EXISTS GameDB.`Mail`
(
    MailId BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '메일 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    Title VARCHAR(50) NOT NULL COMMENT '메일 제목',
    Content VARCHAR(200) NOT NULL COMMENT '메일 내용',
    IsRead BOOL COMMENT NOT NULL COMMENT '읽음 여부',
    IsReceived BOOL COMMENT NOT NULL COMMENT '아이템 수령 여부',
    IsInAppProduct BOOL COMMENT NOT NULL COMMENT '인앱 상품 여부',
    ExpirationTime DATETIME NOT NULL COMMENT '보관 만료 시간'
) COMMENT '우편';
```

## MailItem Table
우편 아이템

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`MailItem`;
CREATE TABLE IF NOT EXISTS GameDB.`MailItem`
(
    MailId BIGINT NOT NULL COMMENT '메일 번호',
    ItemId BIGINT NOT NULL COMMENT '아이템 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수'
) COMMENT '우편';
```

## Attendance Table
출석

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`Attendance`;
CREATE TABLE IF NOT EXISTS GameDB.`Attendance`
(
    AccountId BIGINT NOT NULL PRIMARY KEY COMMENT '계정 번호',
    DateId SMALLINT NOT NULL COMMENT '마지막으로 받은 보상 번호',
    LastAttendanceDate DATETIME NOT NULL COMMENT '마지막으로 출석한 날짜'
) COMMENT '출석부';
```

## Receipt Table
영수증

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`Receipt`;
CREATE TABLE IF NOT EXISTS GameDB.`Receipt`
(
    ReceiptId BIGINT NOT NULL PRIMARY KEY COMMENT '영수증 번호',
    AccountId BIGINT NOT NULL COMMENT '계정 번호',
    ProductId SMALLINT NOT NULL COMMENT '상품 번호'
) COMMENT '인앱 결제 영수증';
```