--liquibase formatted sql

/*
Schema may get created by Liquibase so this step is skipped but just in case.
 */
--changeset CreateSchema:1
CREATE SCHEMA IF NOT EXISTS asuka_net
    DEFAULT CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

/*
Schema gets created with default character set and collation by Liquibase.
This alter schema ensures the collation is indeed utf8mb4_unicode_ci.
 */
--changeset AlterSchema:2
ALTER SCHEMA asuka_net
    DEFAULT CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

--changeset CreateTables:20210208044900
CREATE TABLE tags
(
    id          INT       NOT NULL PRIMARY KEY AUTO_INCREMENT,
    name        CHAR(100) NOT NULL,
    content     CHAR(255) NOT NULL,
    user_id     BIGINT    NOT NULL,
    guild_id    BIGINT    NOT NULL,
    usage_count INT       NOT NULL DEFAULT 0,
    created_at  TIMESTAMP          DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP          DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
/* There cannot be more than one tag with the same name in the same guild. */
    CONSTRAINT unique_per_guild UNIQUE (name, guild_id)
);
