# Jump System Troubleshooting Guide

## 🔍 Common Issues and Solutions

### **1. Ground Detection Problems**

**Issue**: Player can't jump even when on the ground
**Solutions**:
- Check if your ground objects are on the correct layer
- Increase `groundCheckDistance` to 1.2f or higher
- Ensure the ground has colliders attached
- Verify the `groundLayerMask` includes your ground layer

**Debug Steps**:
1. Enable `showGroundCheckGizmos` in the inspector
2. Select your player in the Scene view to see the ground check rays
3. Green rays = ground detected, Red rays = no ground
4. Check the Console for ground check debug messages

### **2. Physics Configuration Issues**

**Issue**: Jump force is applied but player doesn't move
**Solutions**:
- Check Rigidbody settings:
  - Mass: Should be around 1-10
  - Drag: Should be low (0-2)
  - Angular Drag: Around 5
  - Use Gravity: Must be checked
- Increase `jumpForce` value (try 10-15)
- Ensure Rigidbody is not kinematic

### **3. Collider Problems**

**Issue**: Ground detection fails
**Solutions**:
- Player needs a Collider (Capsule Collider recommended)
- Ground objects need Colliders
- Check for overlapping colliders
- Ensure colliders are not set as "Trigger"

### **4. Layer Configuration**

**Issue**: Raycast doesn't hit ground
**Solutions**:
1. Set up ground layer properly:
   - Create a "Ground" layer (Layer 8 is common)
   - Assign ground objects to this layer
   - Set `groundLayerMask` to include the Ground layer

2. Layer Mask Values:
   - `-1` = All layers (good for testing)
   - `1` = Default layer only
   - `256` = Layer 8 (Ground layer)

### **5. Network Ownership Issues**

**Issue**: Jump doesn't work in multiplayer
**Solutions**:
- Ensure you're the owner of the NetworkObject
- Check the `IsOwner` check in FixedUpdate
- Verify NetworkObject is properly spawned

## 🛠 Quick Fixes to Try

### **Fix 1: Increase Detection Range**
```csharp
[SerializeField] private float groundCheckDistance = 1.5f;  // Was 0.1f
```

### **Fix 2: Use All Layers for Testing**
```csharp
[SerializeField] private LayerMask groundLayerMask = -1;  // All layers
```

### **Fix 3: Increase Jump Force**
```csharp
[SerializeField] private float jumpForce = 12f;  // Was 7f
```

### **Fix 4: Add Physics Material**
Create a Physics Material for your player:
- Dynamic Friction: 0.6
- Static Friction: 0.6
- Bounciness: 0
- Friction Combine: Average
- Bounce Combine: Average

## 🧪 Debug Tests

### **Test 1: Manual Jump Trigger**
Add this to PlayerController Update():
```csharp
if (Input.GetKeyDown(KeyCode.T))  // Test key
{
    Debug.Log("Manual jump test");
    GetComponent<Rigidbody>().AddForce(Vector3.up * 15f, ForceMode.VelocityChange);
}
```

### **Test 2: Ground Check Visualization**
The updated PlayerMotor now shows:
- Green/Red rays for ground detection
- Blue spheres for SphereCast detection
- Console messages with ground state

### **Test 3: Rigidbody Inspector**
While playing, watch the Rigidbody component:
- Velocity should change when jumping
- Y velocity should become positive during jump
- Mass and drag values should be reasonable

## 📋 Checklist

Before asking for help, verify:

**Scene Setup**:
- [ ] Player has Rigidbody (not kinematic)
- [ ] Player has Collider
- [ ] Ground has Collider
- [ ] Ground is not marked as Trigger
- [ ] Gravity is enabled in Physics settings

**Component Setup**:
- [ ] PlayerMotor script is attached
- [ ] Jump force is set (try 10-15)
- [ ] Ground check distance is adequate (1.0+)
- [ ] Layer mask includes ground layer

**Network Setup** (for multiplayer):
- [ ] Player is network owner
- [ ] NetworkObject is spawned properly
- [ ] IsOwner returns true for local player

**Input Setup**:
- [ ] Jump input is being detected (check Console)
- [ ] PlayerController calls motor.Jump()
- [ ] Jump method is being called

## 🎯 Most Likely Solutions

**90% of jump issues are caused by**:
1. Ground check distance too small (0.1f → 1.2f)
2. Wrong layer mask (use -1 for all layers)
3. Missing or incorrect colliders
4. Kinematic rigidbody
5. Jump force too low

**Try these in order**:
1. Set `groundLayerMask = -1` (all layers)
2. Set `groundCheckDistance = 1.5f`
3. Set `jumpForce = 15f`
4. Check Rigidbody is not kinematic
5. Verify ground has colliders

If jumping still doesn't work after these fixes, the issue is likely in the scene setup or physics configuration.
