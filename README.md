# 🕹️ AR Laser Tag Visualiser

## 📚 CG4002 Computer Engineering Capstone Project

Welcome to the **Unity AR Laser Tag Game Visualiser**!

![AR View Mode](./PublicImages/ar-view.jpg)

This is the Unity AR application is the frontend of a **5-part group project for CG4002 Computer Engineering Capstone Project** AY24/25 Semester 2! The other components include:
- Hardware
- Internal Communications
- External Communications
- AI on FPGA

This project is a two-player augmented reality (AR) laser tag game developed with **Unity and Vuforia**. By blending mixed reality features, it offers an engaging, hands-free experience where players can compete using both ranged and melee attacks, while keeping track of their health, shields, and ammunition.

The application connects to an **MQTT broker** running on a physical Ultra96 server in the computer lab, receiving live game data. This setup continually updates the game state—such as actions, player health, shields, and ammo—so the app can display the latest information.

For immersive, stereoscopic visuals, the game integrates the **Google Cardboard XR plugin**, allowing users to place their phones into a headset and enjoy the experience in full virtual reality.



## ✨ Features & Game Rules

### Health and Shield System
- Players start with a specific amount of **Health** and **Shield**.
- Once the shield is depleted, damage affects the player's **Health**.
- When health reaches zero, the player loses and the scoreboard is updated.
- Shield Effect when shield is activated.

### Ammo System
- Players have a limited ammo count for their weapons.
- **Bullets** - total of 6 bullets and can be reloaded (Reloading Effect)
- **Bombs** - total of 2 bombs each life (respawn to replenish)
- **Shields** - total of 3 shields each life (respawn to replenish)

### Projectile Effects
- Projectile Shots are implemented for **Badminton Serving**, **Golf Swing** and **Bomb Throw** Actions
- Hits create an **impact effect** on the target, with audio feedback for successful shots.
- Bombs will create an AOE snow animation upon successful impact
- Both players can see their effect on the enemy and their enemy's effect on themselves

### Melee Effects
- Players can perform close-range melee attacks for **Boxing Jabs** and **Fencing Lunge** Actions
- Melee attacks feature distinct **animations** and **impact audios and visuals**
- Both players can see their effect on the enemy and their enemy's effect on themselves

### Gun Shots
- Players can shoot a physical gun and have the corresponding animation on the visualiser
- There will be a corresponding **muzzle flash** and **hit animation** on the enemy

### Snow Bomb
When a snow bomb projectile strikes an opponent, it generates a snowfall zone with a 1-meter radius centered on the point of impact.
- These snow zones remain active within the game environment.
- If an opponent enters an existing snow zone, they receive snow damage.
- Furthermore, any action taken by the opposing player while their target is within a snow zone deals extra damage.

## 🛠️ Getting Started

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yeo-menghan/CG4002-Visualiser.git
   cd CG4002-Visualiser
   ```
2. Install Unity: Download and install Unity Hub and the required version of Unity.
3. Vuforia Setup:
    - Sign up for a free Vuforia Developer Account.
    - Obtain a Vuforia license key and add it to the Unity project.
4. MQTT Broker:
    - Set up an MQTT broker (e.g., [HiveMQ](https://www.hivemq.com/)).
    - Configure the MQTT settings in the Unity project.
5. Build and Run:
    - Open the project in Unity, build the app, and deploy it to your AR-enabled devices.

## 📚 Tech Stack

- Unity: The primary engine powering game development, rendering, and interactive experiences.
- Vuforia: Provides robust AR tracking and seamless integration of virtual content into real-world environments.
- MQTT: Facilitates fast and lightweight real-time data exchange between devices and the game server (Ultra96)
- C#: The scripting language used to implement gameplay mechanics and application logic.
- Google Cardboard XR Plugin: Delivers immersive stereoscopic visuals, enabling mixed reality experiences through mobile VR headsets.

## 🔌 Installation

This project is designed for iOS and is not currently available on the App Store. If you would like access to the project files, please reach out to the project owner.

## Credits
- Pixabay for sound effects
- Sketchfab and Unity Asset Store for in-game prefabs
