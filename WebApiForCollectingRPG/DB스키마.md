# account_db
  
## account Table
홈페이지 계정 정보를 가지고 있는 테이블    
  
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
# master_db

```sql
CREATE DATABASE IF NOT EXISTS master_db;
```
  
## item Table
아이템 정보

```sql

USE master_db;
DROP TABLE IF EXISTS master_db.`item`;
CREATE TABLE IF NOT EXISTS master_db.`item`
(
    item_id BIGINT NOT NULL PRIMARY KEY COMMENT '아이템 번호',
    name VARCHAR(50) UNIQUE NOT NULL COMMENT '아이템 이름',
    attribute_id INT COMMENT  '아이템 특성',
    sell_price BIGINT COMMENT  '파는 가격',
    buy_price BIGINT COMMENT '사는 가격',
    use_lv SMALLINT COMMENT '사용 가능 레벨',
    attack BIGINT COMMENT '공격력',
    defence BIGINT  COMMENT '방어력',
    magic BIGINT COMMENT '마력',
    enhance_max_count SMALLINT COMMENT '최대 강화 횟수', 
    is_item_stackable BOOL COMMENT '아이템 겹침 보관 가능 여부'
) COMMENT '아이템 정보';

insert into Item (item_id, name, attribute_id, sell_price, buy_price, use_lv, attack, defence, magic, enhance_max_count, is_item_stackable) values
(1, "돈", 5, 0, 0, 0, 0, 0, 0, 0, true),
(2, "작은 칼", 1, 10, 20, 1, 10, 5, 1, 10, false),
(3, "도금 칼", 1, 100, 200, 5, 29, 12, 10, 10, false),
(4, "나무 방패", 2, 7, 15, 1, 3, 10, 1, 10, false),
(5, "보통 모자", 3, 5, 8, 1, 1, 1, 1, 10, false),
(6, "포션", 4, 3, 6, 1, 0, 0, 0, 0, true);
```

## item_attribute Table
아이템 특성 정보

```sql
USE master_db;
DROP TABLE IF EXISTS master_db.`item_attribute`;
CREATE TABLE IF NOT EXISTS master_db.`item_attribute`
(
    attribute_id INT NOT NULL PRIMARY KEY COMMENT '특성 번호',
    name VARCHAR(50) UNIQUE NOT NULL COMMENT '특성 이름'
) COMMENT '아이템 특성 정보';

insert into item_attribute (attribute_id, name) values
(1, "무기"),
(2, "방어구"),
(3, "복장"),
(4, "마법도구"),
(5, "돈");
```

## attendance_compensation Table
출석부 보상

```sql
USE master_db;
DROP TABLE IF EXISTS master_db.`attendance_compensation`;
CREATE TABLE IF NOT EXISTS master_db.`attendance_compensation`
(
    compensation_id SMALLINT NOT NULL PRIMARY KEY COMMENT '보상 번호',
    item_id BIGINT NOT NULL COMMENT '아이템 번호',
    item_count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '출석부 보상';

insert into attendance_compensation (compensation_id, item_id, item_count) values
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

## in_app_product Table
인앱 상품

```sql
USE master_db;
DROP TABLE IF EXISTS master_db.`in_app_product`;
CREATE TABLE IF NOT EXISTS master_db.`in_app_product`
(
    product_id SMALLINT NOT NULL COMMENT '상품 번호',
    item_id BIGINT NOT NULL COMMENT '아이템 번호',
    item_name VARCHAR(50) NOT NULL COMMENT '아이템 이름',
    item_count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '인앱 상품 정보';

insert into in_app_product (product_id, item_id, item_name, item_count) values
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


## stage_item Table

```sql
USE master_db;
DROP TABLE IF EXISTS master_db.`stage_item`;
CREATE TABLE IF NOT EXISTS master_db.`stage_item`
(
    stage_item_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '스테이지-아이템 번호',
    stage_id INT NOT NULL COMMENT '스테이지 번호',
    item_id BIGINT NOT NULL COMMENT '아이템 번호'
) COMMENT '스테이지별 아이템 정보';

insert into stage_item (stage_id, item_id) values
(1, 1),
(1, 2),
(2, 3),
(2, 3);
```

## stage_attack_npc Table

```sql
USE master_db;
DROP TABLE IF EXISTS master_db.`stage_attack_npc`;
CREATE TABLE IF NOT EXISTS master_db.`stage_attack_npc`
(
    stage_attack_npc_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '스테이지-공격 NPC 번호',
    stage_id INT NOT NULL COMMENT '스테이지 번호',
    npc_id INT NOT NULL COMMENT '공격 NPC 번호',
    npc_count INT NOT NULL COMMENT '개수',
    exp BIGINT NOT NULL COMMENT '1마리당 보상 경험치'
) COMMENT '스테이지 공격 NPC 정보';

insert into stage_attack_npc (stage_id, npc_id, npc_count, exp) values
(1, 101, 10, 10),
(1, 110, 12, 15),
(2, 201, 40, 20),
(2, 211, 20, 35),
(2, 221, 1, 50);
```

<br>
# game_db

```sql
DROP DATABASE IF EXISTS game_db;
CREATE DATABASE IF NOT EXISTS game_db;
```

## account_player Table
홈페이지 계정에 따른 플레이어 계정 데이터
```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`account_player`;
CREATE TABLE IF NOT EXISTS game_db.`account_player`
(    
    player_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '플레이어 번호',
    account_id BIGINT NOT NULL UNIQUE KEY COMMENT '계정 번호'
) COMMENT '계정-플레이어 데이터';
```
  
## player_game Table
각 플레이어별 게임 데이터

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`player_game`;
CREATE TABLE IF NOT EXISTS game_db.`player_game`
(
    player_id BIGINT NOT NULL PRIMARY KEY COMMENT '플레이어 번호',
    money BIGINT NOT NULL COMMENT '돈',
    exp BIGINT NOT NULL COMMENT '경험치'
) COMMENT '플레이어별 게임 데이터';
```
  
## player_item Table
각 플레이어별 아이템 데이터

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`player_item`;
CREATE TABLE IF NOT EXISTS game_db.`player_item`
(
    player_item_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '플레이어-아이템 번호',
    player_id BIGINT NOT NULL COMMENT '플레이어 번호',
    item_id BIGINT NOT NULL COMMENT '아이템 번호',
    item_count INT NOT NULL COMMENT '아이템 개수',
    enhance_count SMALLINT NOT NULL COMMENT '강화횟수',
    attack BIGINT COMMENT '공격력',
    defence BIGINT  COMMENT '방어력',
    magic BIGINT COMMENT '마력'
) COMMENT '유저별 아이템 데이터';
CREATE INDEX player_id_idx ON game_db.player_item (player_id);
```

## mail Table
우편

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`mail`;
CREATE TABLE IF NOT EXISTS game_db.`mail`
(
    mail_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '메일 번호',
    player_id BIGINT NOT NULL COMMENT '플레이어 번호',
    title VARCHAR(50) NOT NULL COMMENT '메일 제목',
    content VARCHAR(200) NOT NULL COMMENT '메일 내용',
    is_received BOOL NOT NULL COMMENT '아이템 수령 여부',
    is_in_app_product BOOL NOT NULL COMMENT '인앱 상품 여부',
    expiration_time DATETIME NOT NULL COMMENT '보관 만료 시간',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '생성 날짜',
    is_deleted BOOL NOT NULL COMMENT '삭제 여부',
    is_read BOOL NOT NULL COMMENT '읽음 여부',
    has_item BOOL NOT NULL COMMENT '아이템 포함 여부'
) COMMENT '우편';
CREATE INDEX player_id_idx ON game_db.mail (player_id);
```

## mail_item Table
우편 아이템

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`mail_item`;
CREATE TABLE IF NOT EXISTS game_db.`mail_item`
(
    mail_item_id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '메일-아이템 번호',
    mail_id BIGINT NOT NULL COMMENT '메일 번호',
    item_id BIGINT NOT NULL COMMENT '아이템 번호',
    item_count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '우편';
CREATE INDEX mail_id_idx ON game_db.mail_item (mail_id);
```

## attendance Table
출석

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`attendance`;
CREATE TABLE IF NOT EXISTS game_db.`attendance`
(
    player_id BIGINT NOT NULL PRIMARY KEY COMMENT '플레이어 번호',
    last_compensation_id SMALLINT NOT NULL COMMENT '마지막으로 받은 보상 번호',
    last_attendance_date DATETIME NOT NULL COMMENT '마지막으로 출석한 날짜'
) COMMENT '출석부';
```

## receipt Table
영수증

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`receipt`;
CREATE TABLE IF NOT EXISTS game_db.`receipt`
(
    receipt_id BIGINT NOT NULL PRIMARY KEY COMMENT '영수증 번호',
    player_id BIGINT NOT NULL COMMENT '플레이어 번호',
    product_id SMALLINT NOT NULL COMMENT '상품 번호'
) COMMENT '인앱 결제 영수증';
```

## highest_cleared_stage Table
클리어한 가장 높은 스테이지 정보

```sql
USE game_db;
DROP TABLE IF EXISTS game_db.`highest_cleared_stage`;
CREATE TABLE IF NOT EXISTS game_db.`highest_cleared_stage`
(
    player_id BIGINT NOT NULL PRIMARY KEY COMMENT '플레이어 번호',
    stage_id INT NOT NULL COMMENT '클리어한 가장 높은 스테이지 번호'
) COMMENT '클리어한 가장 높은 스테이지 정보';
```