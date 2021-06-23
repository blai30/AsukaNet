# Asuka.Net (Client)

Backend repo here: [Link to API repo](https://www.github.com/blai30/Asuka.Net-Api)

![screenshot](https://github.com/blai30/Asuka.Net/blob/main/resources/asuka.png?raw=true)

## Instructions
1. Copy `appsettings.Example.json` to `appsettings.json`.
2. Edit `appsettings.json` then add bot token and other configurations.
3. Run `dotnet publish -c Release -o App`.
4. Navigate to `/App`.
5. Run `dotnet ./Asuka.dll`.

## Docker
```
docker-compose up -d
```

## Available commands
* Fun
  * `flipcoin`
  * `roll [sides = 6]`
* General
  * `help [command]`
  * `ping`
  * `serverinfo`
  * `userinfo [@user]`
* Music
  * `music join`
  * `music leave`
  * `music play <query|url>`
  * `music pause`
  * `music skip`
  * `music remove <index>`
  * `music queue`
  * `music clear`
* Roles
  * `reactionrole make [description]`
  * `reactionrole add <messageId> <:emoji:> <@role>`
  * `reactionrole remove <messageId> <@role>`
  * `reactionrole edit <messageId> [title] [description]`
  * `reactionrole clear <messageId>`
* Tags
  * `tag add <name> <content> [:reaction:]`
  * `tag remove <name>`
  * `tag edit <name> <content> [:reaction:]`
  * `tag list`
  * `tag info <name>`
* Utility
  * `avatar [@user]`
  * `calculate <expression>`
  * `color <hex|rgb|keywords>`
  * `urban <term>`
