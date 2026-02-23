# 🦾 Unity Robotic Arm Agent

A reinforcement-learning-powered robotic arm simulation built in **Unity** using **ML-Agents**.  
This project demonstrates how a multi-joint arm can autonomously **locate, reach, and grasp** target objects in a 3D environment using continuous control and shaped rewards.

---

## 🎯 Project Overview

This Unity project implements a robotic arm agent that learns to **position its joints** and **control a claw** to reach randomly spawned targets.  
It serves as an exploration of:
- Multi-joint kinematics under continuous control
- Reward shaping and episode management for RL training
- Coordinated movement between articulated segments
- Detecting and responding to success/failure conditions in simulation

The environment is designed for training via **Proximal Policy Optimization (PPO)** through Unity’s ML-Agents Toolkit.

---

## 🤖 Agent Architecture

### **LocateAgent.cs**
The **primary learning agent**, responsible for positioning the robotic arm’s joints to reach a target.

Key components:
- **Joints Controlled:** Base, Shoulder, Elbow, Wrist, Claw rotation  
- **Action Space:** 4 continuous actions (joint rotations)  
- **Observation Space:**  
  - Normalized joint angles  
  - Claw tip position  
  - Target position  
- **Rewards:**  
  - Positive reward for reducing distance to the target  
  - Small bonus when within a success threshold  
  - Negative reward for excessive movement magnitude (energy penalty)  
  - Full reward (+1) on success, penalty (−1) on failure  

The agent resets each episode with randomized target positions to encourage spatial generalization.  
A colored floor (green/red) provides a quick visual cue for success or failure.

---

### **GraspAgent.cs**
A secondary agent used for training **claw control and rotation**, focusing on the grasping motion rather than arm positioning.

Features:
- Continuous control for **rotation** and **claw open/close animation**  
- Reward shaping to encourage closing the claw when near the target  
- Maintains a held object until released  
- Supports heuristic keyboard control for debugging (e.g. Q/W/A/S keys)

The `GraspAgent` can run independently or be combined with `LocateAgent` in curriculum setups.

---

## 🧩 Supporting Systems

- **ClawSuccessDetector.cs** – Detects when the claw collides with an object tagged as `Goal` and triggers a reward/episode end.  
- **ArmFailureDetector.cs** – Ends the episode if the arm collides with the `Ground` to penalize overextension or instability.  
- **ArmLengthMeasurer.cs** – Utility for measuring distance between base and tip, helpful for debugging or calibrating reward distances.  

---

## 🧠 Training Setup

### Environment
- **Engine:** Unity 2021 LTS or newer  
- **ML-Agents:** `mlagents` + `mlagents-envs` (latest verified version)  
- **Python:** 3.9–3.11  
- **Algorithm:** PPO (Proximal Policy Optimization)

### Example Config (`config/robotic_arm.yaml`)
```yaml
behaviors:
  RoboticArm:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024
      buffer_size: 10240
      learning_rate: 3e-4
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 2
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 2.0e6
    time_horizon: 128
    summary_freq: 10000
```

---

## 🏗️ Scene Structure

The environment contains:
- A **robotic arm prefab** with articulated joints driven by ML-Agents  
- A **target object** that spawns randomly within a set radius  
- A **ground plane** acting as a failure boundary  
- Optional sensors or visual indicators for debugging (distance readouts, success/failure colors)

---

## 🚀 Quick Start

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/MooseAlhe/Unity-Robotic-Arm-Agent.git
```

### 2️⃣ Open in Unity
- Use **Unity Hub** → Add project folder → Open.
- Ensure **ML-Agents** and **Barracuda** packages are installed.

### 3️⃣ Set Up Python Environment
```bash
python -m venv .venv
.venv\Scripts\activate      # (Windows)
# or
source .venv/bin/activate     # (macOS/Linux)

pip install --upgrade pip
pip install mlagents mlagents-envs
```

### 4️⃣ Train the Agent
```bash
mlagents-learn config/robotic_arm.yaml --run-id arm_run --time-scale 20
```
Then press **Play** in the Unity Editor when prompted.

### 5️⃣ Visualize Progress
```bash
tensorboard --logdir results
```

---

## 📊 Results & Insights

- The arm learns to **reduce average distance to target** across episodes.  
- Smooth joint motion emerges through continuous control.  
- Reward curves stabilize after sufficient exploration (typically 1–2 million steps).  
- Visual feedback shows the agent efficiently aligning the claw tip to the target’s position.

*(You can add screenshots or a GIF demo here once you have them.)*

---

## 🔍 Future Improvements

- Integrate `GraspAgent` and `LocateAgent` into a single hierarchical controller  
- Add object manipulation (pick-and-place tasks)  
- Include joint torque constraints and physics-based forces  
- Expand observation space with visual input (camera sensors)  
- Tune reward shaping for smoother convergence  

---

## 👤 Author
**MooseAlhe**  
Developed as an experiment in robotic control, Unity ML-Agents, and continuous-space reinforcement learning.
