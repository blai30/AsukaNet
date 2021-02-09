CREATE TABLE tags
(
    id          INT           NOT NULL PRIMARY KEY AUTO_INCREMENT,
    name        NVARCHAR(100) NOT NULL,
    content     NVARCHAR(255) NOT NULL,
    user_id     BIGINT        NOT NULL,
    guild_id    BIGINT        NOT NULL,
    usage_count INT           NOT NULL DEFAULT 0
);
