# CRT Filter for URP (Unity 6 Render Graph)

![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity)
![Render Pipeline](https://img.shields.io/badge/Pipeline-URP-blue)
![API](https://img.shields.io/badge/API-Render%20Graph-green)

This package provides a high-performance, **Render Graph API** compatible CRT (Cathode Ray Tube) post-processing effect designed specifically for Unity 6. Unlike legacy blit-based effects, this system leverages modern transient resource management to minimize GPU memory overhead.

## Core Features
* **Unity 6 Native:** Built from the ground up for the Render Graph API.
* **Screen Geometry:** Authentic CRT curvature (Screen Bend) and overscan controls.
* **Analog Artifacts:** High-quality Gaussian blur, phosphor bleeding, and per-scanline jitter (smidge).
* **Dynamic Effects:** Moving shadowlines, aperture grille simulation, and procedural noise.
* **SOLID Architecture:** Decoupled data (Volume) and logic (Pass) for maximum sustainability.

---

## Installation

### 1. Via Unity Package Manager (UPM)
1.  Open your Unity Project.
2.  Navigate to `Window > Package Manager`.
3.  Click the `+` icon and select **Add package from git URL...**.
4.  Paste the repository URL:
    `https://github.com/Astrobotiq/crt-filter-urp.git`

### 2. Material & Renderer Feature Setup
Since this package provides the raw shader to keep the repository lightweight, you must create a material first:
1.  In your Project window, right-click and select **Create > Material**. Name it `CRT_Material`.
2.  Select the newly created material and change its shader to `Custom/CRTFilter` from the Inspector dropdown.
3.  Locate your project's `Universal Renderer Data` asset.
4.  Click **Add Renderer Feature** and select `Crt Renderer Feature`.
5.  Assign your `CRT_Material` to the material slot in the Renderer Feature.

---

## Parameter Reference

### Screen Geometry
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Screen Bend** | `0.0 - 10.0` | Controls the spherical curvature of the tube. Higher values increase the warp effect. |
| **Screen Overscan** | `0.0 - 0.2` | Adjusts the zoom level to hide or show the distorted screen edges. |
| **Pixel Resolution** | `Vector2` | Sets the virtual downscaled resolution for the pixelation effect (e.g., 320x240). |

### Vignette
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Vignette Size** | `0.0 - 2.0` | Determines the radius/size of the darkened corners. |
| **Vignette Smooth** | `0.0 - 1.0` | Controls the softness and falloff gradient of the vignette edge. |
| **Vignette Round** | `0.0 - 1.0` | Blends between a screen-fitted oval (0.0) and a perfect circle (1.0). |

### Blur Effects
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Blur** | `0.0 - 4.0` | Intensity of the Gaussian blur pass simulating out-of-focus electron beams. |
| **Bleed** | `0.0 - 1.0` | Amount of horizontal phosphor color smearing/bleeding. |
| **Smidge** | `0.0 - 1.0` | Intensity of per-scanline micro-jittering for analog instability. |

### Scanlines and Noise
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Scanlines Strength** | `0.0 - 1.0` | Opacity of the static horizontal scanline grid. |
| **Aperture Strength** | `0.0 - 1.0` | Simulates the vertical RGB aperture grille mask of classic CRT TVs. |
| **Shadowlines** | `0.0 - 1.0` | Thickness/visibility of the vertical rolling dark bar (V-Sync artifact). |
| **Shadowlines Speed**| `0.0 - 5.0` | Travel speed of the rolling shadowlines across the screen. |
| **Shadowlines Alpha**| `0.0 - 1.0` | Transparency of the rolling shadowlines. |
| **Noise Size** | `1.0 - 8.0` | Scale and coarseness of the procedural analog static. |
| **Noise Speed** | `0.0 - 60.0`| How fast the static noise pattern updates. |
| **Noise Alpha** | `0.0 - 1.0` | Master opacity of the procedural analog signal noise. |

### Image Adjustments
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Brightness** | `0.0 - 3.0` | Overall luminance multiplier for the final output. |
| **Contrast** | `0.0 - 3.0` | Contrast ratio scaling for the final image. |
| **Gamma** | `0.1 - 3.0` | Gamma correction curve adjustment. |
| **Red / Green / Blue** | `0.0 - 2.0` | Multipliers for the individual color channels. |

### Chromatic Aberration
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Red Offset** | `-0.05 - 0.05` | Radial displacement of the red color channel. |
| **Green Offset**| `-0.05 - 0.05` | Radial displacement of the green color channel. |
| **Blue Offset** | `-0.05 - 0.05` | Radial displacement of the blue color channel. |

### Global
| Parameter | Range | Description |
| :--- | :--- | :--- |
| **Intensity** | `0.0 - 1.0` | Master weight/opacity of the entire CRT effect over the original camera image. |

---

## Technical Architecture

The system utilizes the **Render Graph API** to manage resources efficiently. 

* **Two-Pass Execution:** The effect renders to a transient `CRTTempTexture` before blitting back to the active color buffer, ensuring compatibility with other post-processing effects.
* **Resource Management:** Textures are allocated and released automatically by the Render Graph system, preventing memory leaks and frame-buffer contention.
* **Optimized Shader:** The fragment shader uses branchless logic where possible and relies on the URP Core library for cross-platform compatibility.

---

## Repository Structure
```text
CrtEffect/
├── Runtime/
│   ├── CrtFilterVolume.cs      # Volume component data
│   ├── CrtRendererFeature.cs   # Renderer Feature injector
│   └── CrtRenderPass.cs        # Render Graph logic
├── Shaders/
│   └── CRTFilter.shader        # HLSL fragment shader
├── package.json                # UPM package manifest
└── README.md                   # Documentation
