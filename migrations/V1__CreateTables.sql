CREATE TABLE tags
(
    id          INT             NOT NULL PRIMARY KEY AUTO_INCREMENT,
    name        VARCHAR(100)    NOT NULL,
    content     VARCHAR(255)    NOT NULL,
    reaction    VARCHAR(100),
    guild_id    BIGINT UNSIGNED NOT NULL,
    user_id     BIGINT UNSIGNED NOT NULL,
    usage_count INT             NOT NULL DEFAULT 0,
    created_at  TIMESTAMP                DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP                DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
/* There cannot be more than one tag with the same name in the same guild. */
    CONSTRAINT unique_per_guild UNIQUE (name, guild_id)
);

CREATE TABLE reaction_roles
(
    id         INT             NOT NULL PRIMARY KEY AUTO_INCREMENT,
    guild_id   BIGINT UNSIGNED NOT NULL,
    channel_id BIGINT UNSIGNED NOT NULL,
    message_id BIGINT UNSIGNED NOT NULL,
    role_id    BIGINT UNSIGNED NOT NULL,
    reaction   VARCHAR(100)    NOT NULL
);
