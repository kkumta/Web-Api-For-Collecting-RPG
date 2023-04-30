# AccountDB
  
## Account 테이블
게임에서 생성 된 계정 정보들을 가지고 있는 테이블    
  
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
  
## Item 테이블
아이템 정보

```sql

USE MasterDB;
DROP TABLE IF EXISTS MasterDB.`Item`;
CREATE TABLE IF NOT EXISTS MasterDB.`Item`
(
    ItemId BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
    Name VARCHAR(50) UNIQUE NOT NULL COMMENT '아이템 이름',
    Attribute INT COMMENT  '아이템 특성',
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

## ItemAttribute 테이블
아이템 특성 정보

```sql
USE MasterDB;
DROP TABLE IF EXISTS MasterDB.`ItemAttribute`;
CREATE TABLE IF NOT EXISTS MasterDB.`ItemAttribute`
(
    AttributeId BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '특성 번호',
    Name VARCHAR(50) UNIQUE NOT NULL COMMENT '특성 이름'
) COMMENT '아이템 특성 정보';

insert into ItemAttribute (Name) values
("무기"),
("방어구"),
("복장"),
("마법도구"),
("돈");
```


# GameDB

```sql
DROP DATABASE IF EXISTS GameDB;
CREATE DATABASE IF NOT EXISTS GameDB;
```
  
## AccountGame 테이블
각 유저별 게임 데이터

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`AccountGame`;
CREATE TABLE IF NOT EXISTS GameDB.`AccountGame`
(
    AccountId BIGINT UNSIGNED NOT NULL PRIMARY KEY COMMENT '계정 번호',
    Money BIGINT UNSIGNED NOT NULL COMMENT '돈',
    Exp BIGINT UNSIGNED NOT NULL COMMENT '경험치'
) COMMENT '유저별 게임 데이터';
```
  
## AccountItem 테이블
각 유저별 아이템 데이터

```sql
USE GameDB;
DROP TABLE IF EXISTS GameDB.`AccountItem`;
CREATE TABLE IF NOT EXISTS GameDB.`AccountItem`
(
    AccountItemId BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정-아이템 번호',
    AccountId BIGINT UNSIGNED NOT NULL COMMENT '계정 번호',
    SlotId SMALLINT UNSIGNED NOT NULL COMMENT '아이템 슬롯 번호',
    ItemId BIGINT UNSIGNED NOT NULL COMMENT '아이템 번호',
    ItemCount INT UNSIGNED NOT NULL COMMENT '아이템 개수',
    EnhanceCount SMALLINT NOT NULL COMMENT '강화횟수'
) COMMENT '유저별 아이템 데이터';
```
