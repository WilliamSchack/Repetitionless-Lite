**CHANGELOG**

- Major performance improvements depending on material and hardware setup
- Added terrain instancing support for
  - BIRP & URP in all unity versions
  - HDRP in Unity 6.3+
- Many various improvements and fixes

**This update includes breaking changes but most will be automatically handled when updating. This includes:**

**Automatically Updated**
- BIRP & URP shaders replaced with shader code
- Layered shader split into LayeredTerrain & LayeredLit
- New shader keywords have been added

**Require Manual Updating**
- SubGraphs will need to be re-added to shaders using them
- Shader code has moved to "Shaders/Common". Paths in shaders referencing them will need to be updated
