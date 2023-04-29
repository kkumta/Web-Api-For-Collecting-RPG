# Account DB
  
## account 테이블
게임에서 생성 된 계정 정보들을 가지고 있는 테이블    
  
```sql
DROP DATABASE IF EXISTS account_db;
CREATE DATABASE IF NOT EXISTS account_db;

USE account_db;

DROP TABLE IF EXISTS account_db.`account`;
CREATE TABLE IF NOT EXISTS account_db.`account`
(
    account_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정 번호',
    email VARCHAR(50) NOT NULL UNIQUE COMMENT '이메일',
    salt_value VARCHAR(100) NOT NULL COMMENT  '암호화 값',
    hashed_password VARCHAR(100) NOT NULL COMMENT '해싱된 비밀번호',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '생성 날짜'
) COMMENT '계정 정보 테이블';
```   
   
<br>  
<br>  
   

# Master DB

```sql
CREATE DATABASE IF NOT EXISTS master_db;
```
  
## item 테이블
아이템 정보

```sql

USE master_db;
DROP TABLE IF EXISTS master_db.`item`;
CREATE TABLE IF NOT EXISTS master_db.`item`
(
    item_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
    name VARCHAR(50) UNIQUE NOT NULL COMMENT '아이템 이름',
    attribute INT COMMENT  '아이템 특성',
    sell_price BIGINT COMMENT  '파는 가격',
    buy_price BIGINT COMMENT '사는 가격',
    use_lv SMALLINT COMMENT '사용 가능 레벨',
    attack BIGINT COMMENT '공격력',
    defence BIGINT  COMMENT '방어력',
    magic BIGINT COMMENT '마력',
    enhance_max_count SMALLINT COMMENT '최대 강화 횟수', 
    is_item_stackable BOOL COMMENT '아이템 겹침 보관 가능 여부'

) COMMENT '아이템 정보';

insert into item (name, attribute, sell_price, buy_price, use_lv, attack, defence, magic, enhance_max_count, is_item_stackable) values
("돈", 5, 0, 0, 0, 0, 0, 0, 0, true),
("작은 칼", 1, 10, 20, 1, 10, 5, 1, 10, false),
("도금 칼", 1, 100, 200, 5, 29, 12, 10, 10, false),
("나무 방패", 2, 7, 15, 1, 3, 10, 1, 10, false),
("보통 모자", 3, 5, 8, 1, 1, 1, 1, 10, false),
("포션", 4, 3, 6, 1, 0, 0, 0, 0, true);
```

## item_attribute 테이블
아이템 특성 정보

```sql

USE master_db;
DROP TABLE IF EXISTS master_db.`item_attribute`;
CREATE TABLE IF NOT EXISTS master_db.`item_attribute`
(
    attribute_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '특성 번호',
    name VARCHAR(50) UNIQUE NOT NULL COMMENT '특성 이름'
) COMMENT '아이템 특성 정보';

insert into item_attribute (name) values
("무기"),
("방어구"),
("복장"),
("마법도구"),
("돈");
```


# Game DB

```sql
CREATE DATABASE IF NOT EXISTS game_db;
```
  
## account_game 테이블
각 유저별 게임 데이터

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`account_game`;
CREATE TABLE IF NOT EXISTS game_db.`account_game`
(
    account_id BIGINT UNSIGNED NOT NULL PRIMARY KEY COMMENT '계정 번호',
    money BIGINT UNSIGNED NOT NULL COMMENT '돈',
    exp BIGINT UNSIGNED NOT NULL COMMENT '경험치'
) COMMENT '유저별 게임 데이터';
```
  
## item_data 테이블
각 유저별 아이템 데이터

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`account_item`;
CREATE TABLE IF NOT EXISTS game_db.`account_item`
(
    account_item_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '계정-아이템 번호',
    account_id BIGINT UNSIGNED NOT NULL COMMENT '계정 번호',
    slot_id SMALLINT UNSIGNED NOT NULL COMMENT '아이템 슬롯 번호',
    item_id BIGINT UNSIGNED NOT NULL COMMENT '아이템 번호',
    item_count INT UNSIGNED NOT NULL COMMENT '아이템 개수',
    enhance_count SMALLINT NOT NULL COMMENT '강화횟수'
) COMMENT '유저별 아이템 데이터';
```
