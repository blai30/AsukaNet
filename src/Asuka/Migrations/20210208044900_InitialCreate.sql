CREATE TABLE tags
(
    id          INT           NOT NULL PRIMARY KEY AUTO_INCREMENT,
    name        NVARCHAR(100) NOT NULL,
    content     NVARCHAR(255) NOT NULL,
    user_id     BIGINT        NOT NULL,
    guild_id    BIGINT        NOT NULL,
    usage_count INT           NOT NULL DEFAULT 0,
    created_at  TIMESTAMP              DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP              DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
