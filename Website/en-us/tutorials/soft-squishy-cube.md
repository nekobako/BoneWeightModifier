# Soft, Squishy Cube
This page explains how to add bone weights to the cube to make it soft and squishy.

1. Right-click the avatar root and create a cube from `3D Object > Cube`.

![Create Cube](../images/tutorials/soft-squishy-cube/create-cube.png)

2. Create nested empty Game Objects under the cube.  
Place the parent Game Object inside the cube, and the child one at the top of the cube.

![Create Game Objects](../images/tutorials/soft-squishy-cube/create-game-objects.png)

3. Add the `VRC Phys Bone` component to the parent Game Object.

![Add VRC Phys Bone](../images/tutorials/soft-squishy-cube/add-vrc-phys-bone.png)

4. Adjust settings such as `Forces > Pull` and `Forces > Spring` as you like.

![VRC Phys Bone の設定](../images/tutorials/soft-squishy-cube/configure-vrc-phys-bone.png)

5. Add the `Bone Weight Modifier` component to the child Game Object.

![Add Bone Weight Modifier](../images/tutorials/soft-squishy-cube/add-bone-weight-modifier.png)

6. Set the `Renderer` to the cube's `Mesh Renderer`.  
In this case, leave the `Bone` unset to apply the weight for this Game Object.

![Configure Bone Weight Modifier](../images/tutorials/soft-squishy-cube/configure-bone-weight-modifier.png)

7. Press the `+` button to add four `Volume` weight.

![Add Volume Weights](../images/tutorials/soft-squishy-cube/add-volume-weights.png)

8. Set the `Position` of each one so that they are located at the top corners of the cube.

![Configure Volume Weights](../images/tutorials/soft-squishy-cube/configure-volume-weights.png)

9. Enter Play Mode to confirm that the cube behaves in a soft, squishy way in the Game View.

<video muted autoplay loop playsinline src="../videos/tutorials/soft-squishy-cube/soft-squishy-cube.mp4"></video>
