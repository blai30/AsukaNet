CREATE TABLE tags
(
    id          INT           NOT NULL PRIMARY KEY,
    name        NVARCHAR(200) NOT NULL,
    content     NVARCHAR(500) NOT NULL,
    user_id     BIGINT        NOT NULL,
    guild_id    BIGINT        NOT NULL,
    usage_count INT           NOT NULL DEFAULT 0
);
