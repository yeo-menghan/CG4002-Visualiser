## MQTT Communication

The game leverages **MQTT** for real-time communication between players. Key features include:

- **Player State Synchronization**: Health, shield, ammo, and position updates are sent as MQTT messages.
- **Topics**:
  - `visualiser/req_visibility`: Subscribes to External Communications request for Visualizer's visibility
    ```json
    {
      "player_id": 1,
      "topic": "visualiser/req_visibility"
    }
    ```
  - `visualiser/visibility_feedback`: Publishes Player's visibility on the opposing player as well as the number of bombs the opposing player is standing on
    ```json
    {
      "player_id": 1,
      "is_visible": "false",
      "bombs_on_player": 2
    }
    ```
  - `visualiser/game_state`: Subscribes to External Communications' gamestate and update visual components / trigger game actions

    Example of shooting game action
    ```json
    {
      "player_id": 2,
      "action": "shoot",
      "hit": true,
      "game_state": {
        "p1": {
          "hp": 100,
          "bullets": 0,
          "bombs": 1,
          "shield_hp": 0,
          "deaths": 0,
          "shields": 2
        },
        "p2": {
          "hp": 55,
          "bullets": 6,
          "bombs": 2,
          "shield_hp": 0,
          "deaths": 0,
          "shields": 3
        }
      },
      "topic": "visualiser/game_state"
    }
    ```
