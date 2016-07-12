/// <reference path="../../../dist/preview release/babylon.d.ts"/>
/// <reference path="../simple/babylon.simpleMaterial.ts"/>
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var BABYLON;
(function (BABYLON) {
    var maxSimultaneousLights = 4;
    var WaterMaterialDefines = (function (_super) {
        __extends(WaterMaterialDefines, _super);
        function WaterMaterialDefines() {
            _super.call(this);
            this.BUMP = false;
            this.REFLECTION = false;
            this.CLIPPLANE = false;
            this.ALPHATEST = false;
            this.POINTSIZE = false;
            this.FOG = false;
            this.LIGHT0 = false;
            this.LIGHT1 = false;
            this.LIGHT2 = false;
            this.LIGHT3 = false;
            this.SPOTLIGHT0 = false;
            this.SPOTLIGHT1 = false;
            this.SPOTLIGHT2 = false;
            this.SPOTLIGHT3 = false;
            this.HEMILIGHT0 = false;
            this.HEMILIGHT1 = false;
            this.HEMILIGHT2 = false;
            this.HEMILIGHT3 = false;
            this.DIRLIGHT0 = false;
            this.DIRLIGHT1 = false;
            this.DIRLIGHT2 = false;
            this.DIRLIGHT3 = false;
            this.POINTLIGHT0 = false;
            this.POINTLIGHT1 = false;
            this.POINTLIGHT2 = false;
            this.POINTLIGHT3 = false;
            this.SHADOW0 = false;
            this.SHADOW1 = false;
            this.SHADOW2 = false;
            this.SHADOW3 = false;
            this.SHADOWS = false;
            this.SHADOWVSM0 = false;
            this.SHADOWVSM1 = false;
            this.SHADOWVSM2 = false;
            this.SHADOWVSM3 = false;
            this.SHADOWPCF0 = false;
            this.SHADOWPCF1 = false;
            this.SHADOWPCF2 = false;
            this.SHADOWPCF3 = false;
            this.NORMAL = false;
            this.UV1 = false;
            this.UV2 = false;
            this.VERTEXCOLOR = false;
            this.VERTEXALPHA = false;
            this.BONES = false;
            this.BONES4 = false;
            this.BonesPerMesh = 0;
            this.INSTANCES = false;
            this.SPECULARTERM = false;
            this._keys = Object.keys(this);
        }
        return WaterMaterialDefines;
    }(BABYLON.MaterialDefines));
    var WaterMaterial = (function (_super) {
        __extends(WaterMaterial, _super);
        /**
        * Constructor
        */
        function WaterMaterial(name, scene, renderTargetSize) {
            if (renderTargetSize === void 0) { renderTargetSize = new BABYLON.Vector2(512, 512); }
            _super.call(this, name, scene);
            this.renderTargetSize = renderTargetSize;
            this.diffuseColor = new BABYLON.Color3(1, 1, 1);
            this.specularColor = new BABYLON.Color3(0, 0, 0);
            this.specularPower = 64;
            this.disableLighting = false;
            /**
            * @param {number}: Represents the wind force
            */
            this.windForce = 6;
            /**
            * @param {Vector2}: The direction of the wind in the plane (X, Z)
            */
            this.windDirection = new BABYLON.Vector2(0, 1);
            /**
            * @param {number}: Wave height, represents the height of the waves
            */
            this.waveHeight = 0.4;
            /**
            * @param {number}: Bump height, represents the bump height related to the bump map
            */
            this.bumpHeight = 0.4;
            /**
            * @param {number}: The water color blended with the reflection and refraction samplers
            */
            this.waterColor = new BABYLON.Color3(0.1, 0.1, 0.6);
            /**
            * @param {number}: The blend factor related to the water color
            */
            this.colorBlendFactor = 0.2;
            /**
            * @param {number}: Represents the maximum length of a wave
            */
            this.waveLength = 0.1;
            /**
            * @param {number}: Defines the waves speed
            */
            this.waveSpeed = 1.0;
            /*
            * Private members
            */
            this._mesh = null;
            this._reflectionTransform = BABYLON.Matrix.Zero();
            this._lastTime = 0;
            this._scaledDiffuse = new BABYLON.Color3();
            this._scaledSpecular = new BABYLON.Color3();
            this._defines = new WaterMaterialDefines();
            this._cachedDefines = new WaterMaterialDefines();
            // Create render targets
            this._createRenderTargets(scene, renderTargetSize);
        }
        Object.defineProperty(WaterMaterial.prototype, "refractionTexture", {
            // Get / Set
            get: function () {
                return this._refractionRTT;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(WaterMaterial.prototype, "reflectionTexture", {
            get: function () {
                return this._reflectionRTT;
            },
            enumerable: true,
            configurable: true
        });
        // Methods
        WaterMaterial.prototype.addToRenderList = function (node) {
            this._refractionRTT.renderList.push(node);
            this._reflectionRTT.renderList.push(node);
        };
        WaterMaterial.prototype.enableRenderTargets = function (enable) {
            var refreshRate = enable ? 1 : 0;
            this._refractionRTT.refreshRate = refreshRate;
            this._reflectionRTT.refreshRate = refreshRate;
        };
        WaterMaterial.prototype.needAlphaBlending = function () {
            return (this.alpha < 1.0);
        };
        WaterMaterial.prototype.needAlphaTesting = function () {
            return false;
        };
        WaterMaterial.prototype.getAlphaTestTexture = function () {
            return null;
        };
        WaterMaterial.prototype._checkCache = function (scene, mesh, useInstances) {
            if (!mesh) {
                return true;
            }
            if (this._defines.INSTANCES !== useInstances) {
                return false;
            }
            if (mesh._materialDefines && mesh._materialDefines.isEqual(this._defines)) {
                return true;
            }
            return false;
        };
        WaterMaterial.prototype.isReady = function (mesh, useInstances) {
            if (this.checkReadyOnlyOnce) {
                if (this._wasPreviouslyReady) {
                    return true;
                }
            }
            var scene = this.getScene();
            if (!this.checkReadyOnEveryCall) {
                if (this._renderId === scene.getRenderId()) {
                    if (this._checkCache(scene, mesh, useInstances)) {
                        return true;
                    }
                }
            }
            var engine = scene.getEngine();
            var needNormals = false;
            var needUVs = false;
            this._defines.reset();
            // Textures
            if (scene.texturesEnabled) {
                if (this.bumpTexture && BABYLON.StandardMaterial.BumpTextureEnabled) {
                    if (!this.bumpTexture.isReady()) {
                        return false;
                    }
                    else {
                        needUVs = true;
                        this._defines.BUMP = true;
                    }
                }
                if (BABYLON.StandardMaterial.ReflectionTextureEnabled) {
                    this._defines.REFLECTION = true;
                }
            }
            // Effect
            if (scene.clipPlane) {
                this._defines.CLIPPLANE = true;
            }
            if (engine.getAlphaTesting()) {
                this._defines.ALPHATEST = true;
            }
            // Point size
            if (this.pointsCloud || scene.forcePointsCloud) {
                this._defines.POINTSIZE = true;
            }
            // Fog
            if (scene.fogEnabled && mesh && mesh.applyFog && scene.fogMode !== BABYLON.Scene.FOGMODE_NONE && this.fogEnabled) {
                this._defines.FOG = true;
            }
            var lightIndex = 0;
            if (scene.lightsEnabled && !this.disableLighting) {
                for (var index = 0; index < scene.lights.length; index++) {
                    var light = scene.lights[index];
                    if (!light.isEnabled()) {
                        continue;
                    }
                    // Excluded check
                    if (light._excludedMeshesIds.length > 0) {
                        for (var excludedIndex = 0; excludedIndex < light._excludedMeshesIds.length; excludedIndex++) {
                            var excludedMesh = scene.getMeshByID(light._excludedMeshesIds[excludedIndex]);
                            if (excludedMesh) {
                                light.excludedMeshes.push(excludedMesh);
                            }
                        }
                        light._excludedMeshesIds = [];
                    }
                    // Included check
                    if (light._includedOnlyMeshesIds.length > 0) {
                        for (var includedOnlyIndex = 0; includedOnlyIndex < light._includedOnlyMeshesIds.length; includedOnlyIndex++) {
                            var includedOnlyMesh = scene.getMeshByID(light._includedOnlyMeshesIds[includedOnlyIndex]);
                            if (includedOnlyMesh) {
                                light.includedOnlyMeshes.push(includedOnlyMesh);
                            }
                        }
                        light._includedOnlyMeshesIds = [];
                    }
                    if (!light.canAffectMesh(mesh)) {
                        continue;
                    }
                    needNormals = true;
                    this._defines["LIGHT" + lightIndex] = true;
                    var type;
                    if (light instanceof BABYLON.SpotLight) {
                        type = "SPOTLIGHT" + lightIndex;
                    }
                    else if (light instanceof BABYLON.HemisphericLight) {
                        type = "HEMILIGHT" + lightIndex;
                    }
                    else if (light instanceof BABYLON.PointLight) {
                        type = "POINTLIGHT" + lightIndex;
                    }
                    else {
                        type = "DIRLIGHT" + lightIndex;
                    }
                    this._defines[type] = true;
                    // Specular
                    if (!light.specular.equalsFloats(0, 0, 0)) {
                        this._defines.SPECULARTERM = true;
                    }
                    // Shadows
                    if (scene.shadowsEnabled) {
                        var shadowGenerator = light.getShadowGenerator();
                        if (mesh && mesh.receiveShadows && shadowGenerator) {
                            this._defines["SHADOW" + lightIndex] = true;
                            this._defines.SHADOWS = true;
                            if (shadowGenerator.useVarianceShadowMap || shadowGenerator.useBlurVarianceShadowMap) {
                                this._defines["SHADOWVSM" + lightIndex] = true;
                            }
                            if (shadowGenerator.usePoissonSampling) {
                                this._defines["SHADOWPCF" + lightIndex] = true;
                            }
                        }
                    }
                    lightIndex++;
                    if (lightIndex === maxSimultaneousLights)
                        break;
                }
            }
            // Attribs
            if (mesh) {
                if (needNormals && mesh.isVerticesDataPresent(BABYLON.VertexBuffer.NormalKind)) {
                    this._defines.NORMAL = true;
                }
                if (needUVs) {
                    if (mesh.isVerticesDataPresent(BABYLON.VertexBuffer.UVKind)) {
                        this._defines.UV1 = true;
                    }
                    if (mesh.isVerticesDataPresent(BABYLON.VertexBuffer.UV2Kind)) {
                        this._defines.UV2 = true;
                    }
                }
                if (mesh.useVertexColors && mesh.isVerticesDataPresent(BABYLON.VertexBuffer.ColorKind)) {
                    this._defines.VERTEXCOLOR = true;
                    if (mesh.hasVertexAlpha) {
                        this._defines.VERTEXALPHA = true;
                    }
                }
                if (mesh.useBones && mesh.computeBonesUsingShaders) {
                    this._defines.BONES = true;
                    this._defines.BonesPerMesh = (mesh.skeleton.bones.length + 1);
                    this._defines.BONES4 = true;
                }
                // Instances
                if (useInstances) {
                    this._defines.INSTANCES = true;
                }
            }
            this._mesh = mesh;
            // Get correct effect      
            if (!this._defines.isEqual(this._cachedDefines)) {
                this._defines.cloneTo(this._cachedDefines);
                scene.resetCachedMaterial();
                // Fallbacks
                var fallbacks = new BABYLON.EffectFallbacks();
                if (this._defines.FOG) {
                    fallbacks.addFallback(1, "FOG");
                }
                for (lightIndex = 0; lightIndex < maxSimultaneousLights; lightIndex++) {
                    if (!this._defines["LIGHT" + lightIndex]) {
                        continue;
                    }
                    if (lightIndex > 0) {
                        fallbacks.addFallback(lightIndex, "LIGHT" + lightIndex);
                    }
                    if (this._defines["SHADOW" + lightIndex]) {
                        fallbacks.addFallback(0, "SHADOW" + lightIndex);
                    }
                    if (this._defines["SHADOWPCF" + lightIndex]) {
                        fallbacks.addFallback(0, "SHADOWPCF" + lightIndex);
                    }
                    if (this._defines["SHADOWVSM" + lightIndex]) {
                        fallbacks.addFallback(0, "SHADOWVSM" + lightIndex);
                    }
                }
                if (this._defines.BONES4) {
                    fallbacks.addFallback(0, "BONES4");
                }
                //Attributes
                var attribs = [BABYLON.VertexBuffer.PositionKind];
                if (this._defines.NORMAL) {
                    attribs.push(BABYLON.VertexBuffer.NormalKind);
                }
                if (this._defines.UV1) {
                    attribs.push(BABYLON.VertexBuffer.UVKind);
                }
                if (this._defines.UV2) {
                    attribs.push(BABYLON.VertexBuffer.UV2Kind);
                }
                if (this._defines.VERTEXCOLOR) {
                    attribs.push(BABYLON.VertexBuffer.ColorKind);
                }
                if (this._defines.BONES) {
                    attribs.push(BABYLON.VertexBuffer.MatricesIndicesKind);
                    attribs.push(BABYLON.VertexBuffer.MatricesWeightsKind);
                }
                if (this._defines.INSTANCES) {
                    attribs.push("world0");
                    attribs.push("world1");
                    attribs.push("world2");
                    attribs.push("world3");
                }
                // Legacy browser patch
                var shaderName = "water";
                var join = this._defines.toString();
                this._effect = scene.getEngine().createEffect(shaderName, attribs, ["world", "view", "viewProjection", "vEyePosition", "vLightsType", "vDiffuseColor", "vSpecularColor",
                    "vLightData0", "vLightDiffuse0", "vLightSpecular0", "vLightDirection0", "vLightGround0", "lightMatrix0",
                    "vLightData1", "vLightDiffuse1", "vLightSpecular1", "vLightDirection1", "vLightGround1", "lightMatrix1",
                    "vLightData2", "vLightDiffuse2", "vLightSpecular2", "vLightDirection2", "vLightGround2", "lightMatrix2",
                    "vLightData3", "vLightDiffuse3", "vLightSpecular3", "vLightDirection3", "vLightGround3", "lightMatrix3",
                    "vFogInfos", "vFogColor", "pointSize",
                    "vNormalInfos",
                    "mBones",
                    "vClipPlane", "normalMatrix",
                    "shadowsInfo0", "shadowsInfo1", "shadowsInfo2", "shadowsInfo3",
                    // Water
                    "worldReflectionViewProjection", "windDirection", "waveLength", "time", "windForce",
                    "cameraPosition", "bumpHeight", "waveHeight", "waterColor", "colorBlendFactor", "waveSpeed"
                ], ["normalSampler",
                    "shadowSampler0", "shadowSampler1", "shadowSampler2", "shadowSampler3",
                    // Water
                    "refractionSampler", "reflectionSampler"
                ], join, fallbacks, this.onCompiled, this.onError);
            }
            if (!this._effect.isReady()) {
                return false;
            }
            this._renderId = scene.getRenderId();
            this._wasPreviouslyReady = true;
            if (mesh) {
                if (!mesh._materialDefines) {
                    mesh._materialDefines = new WaterMaterialDefines();
                }
                this._defines.cloneTo(mesh._materialDefines);
            }
            return true;
        };
        WaterMaterial.prototype.bindOnlyWorldMatrix = function (world) {
            this._effect.setMatrix("world", world);
        };
        WaterMaterial.prototype.bind = function (world, mesh) {
            var scene = this.getScene();
            // Matrices        
            this.bindOnlyWorldMatrix(world);
            this._effect.setMatrix("viewProjection", scene.getTransformMatrix());
            // Bones
            if (mesh && mesh.useBones && mesh.computeBonesUsingShaders) {
                this._effect.setMatrices("mBones", mesh.skeleton.getTransformMatrices(mesh));
            }
            if (scene.getCachedMaterial() !== this) {
                // Textures        
                if (this.bumpTexture && BABYLON.StandardMaterial.BumpTextureEnabled) {
                    this._effect.setTexture("normalSampler", this.bumpTexture);
                    this._effect.setFloat2("vNormalInfos", this.bumpTexture.coordinatesIndex, this.bumpTexture.level);
                    this._effect.setMatrix("normalMatrix", this.bumpTexture.getTextureMatrix());
                }
                // Clip plane
                if (scene.clipPlane) {
                    var clipPlane = scene.clipPlane;
                    this._effect.setFloat4("vClipPlane", clipPlane.normal.x, clipPlane.normal.y, clipPlane.normal.z, clipPlane.d);
                }
                // Point size
                if (this.pointsCloud) {
                    this._effect.setFloat("pointSize", this.pointSize);
                }
                this._effect.setVector3("vEyePosition", scene._mirroredCameraPosition ? scene._mirroredCameraPosition : scene.activeCamera.position);
            }
            this._effect.setColor4("vDiffuseColor", this._scaledDiffuse, this.alpha * mesh.visibility);
            if (this._defines.SPECULARTERM) {
                this._effect.setColor4("vSpecularColor", this.specularColor, this.specularPower);
            }
            if (scene.lightsEnabled && !this.disableLighting) {
                var lightIndex = 0;
                for (var index = 0; index < scene.lights.length; index++) {
                    var light = scene.lights[index];
                    if (!light.isEnabled()) {
                        continue;
                    }
                    if (!light.canAffectMesh(mesh)) {
                        continue;
                    }
                    if (light instanceof BABYLON.PointLight) {
                        // Point Light
                        light.transferToEffect(this._effect, "vLightData" + lightIndex);
                    }
                    else if (light instanceof BABYLON.DirectionalLight) {
                        // Directional Light
                        light.transferToEffect(this._effect, "vLightData" + lightIndex);
                    }
                    else if (light instanceof BABYLON.SpotLight) {
                        // Spot Light
                        light.transferToEffect(this._effect, "vLightData" + lightIndex, "vLightDirection" + lightIndex);
                    }
                    else if (light instanceof BABYLON.HemisphericLight) {
                        // Hemispheric Light
                        light.transferToEffect(this._effect, "vLightData" + lightIndex, "vLightGround" + lightIndex);
                    }
                    light.diffuse.scaleToRef(light.intensity, this._scaledDiffuse);
                    this._effect.setColor4("vLightDiffuse" + lightIndex, this._scaledDiffuse, light.range);
                    if (this._defines.SPECULARTERM) {
                        light.specular.scaleToRef(light.intensity, this._scaledSpecular);
                        this._effect.setColor3("vLightSpecular" + lightIndex, this._scaledSpecular);
                    }
                    // Shadows
                    if (scene.shadowsEnabled) {
                        var shadowGenerator = light.getShadowGenerator();
                        if (mesh.receiveShadows && shadowGenerator) {
                            this._effect.setMatrix("lightMatrix" + lightIndex, shadowGenerator.getTransformMatrix());
                            this._effect.setTexture("shadowSampler" + lightIndex, shadowGenerator.getShadowMapForRendering());
                            this._effect.setFloat3("shadowsInfo" + lightIndex, shadowGenerator.getDarkness(), shadowGenerator.getShadowMap().getSize().width, shadowGenerator.bias);
                        }
                    }
                    lightIndex++;
                    if (lightIndex === maxSimultaneousLights)
                        break;
                }
            }
            // View
            if (scene.fogEnabled && mesh.applyFog && scene.fogMode !== BABYLON.Scene.FOGMODE_NONE) {
                this._effect.setMatrix("view", scene.getViewMatrix());
            }
            // Fog
            if (scene.fogEnabled && mesh.applyFog && scene.fogMode !== BABYLON.Scene.FOGMODE_NONE) {
                this._effect.setFloat4("vFogInfos", scene.fogMode, scene.fogStart, scene.fogEnd, scene.fogDensity);
                this._effect.setColor3("vFogColor", scene.fogColor);
            }
            // Water
            if (BABYLON.StandardMaterial.ReflectionTextureEnabled) {
                this._effect.setTexture("refractionSampler", this._refractionRTT);
                this._effect.setTexture("reflectionSampler", this._reflectionRTT);
            }
            var wrvp = this._mesh.getWorldMatrix().multiply(this._reflectionTransform).multiply(scene.getProjectionMatrix());
            this._lastTime += scene.getEngine().getDeltaTime();
            this._effect.setMatrix("worldReflectionViewProjection", wrvp);
            this._effect.setVector2("windDirection", this.windDirection);
            this._effect.setFloat("waveLength", this.waveLength);
            this._effect.setFloat("time", this._lastTime / 100000);
            this._effect.setFloat("windForce", this.windForce);
            this._effect.setFloat("waveHeight", this.waveHeight);
            this._effect.setFloat("bumpHeight", this.bumpHeight);
            this._effect.setColor4("waterColor", this.waterColor, 1.0);
            this._effect.setFloat("colorBlendFactor", this.colorBlendFactor);
            this._effect.setFloat("waveSpeed", this.waveSpeed);
            _super.prototype.bind.call(this, world, mesh);
        };
        WaterMaterial.prototype._createRenderTargets = function (scene, renderTargetSize) {
            var _this = this;
            // Render targets
            this._refractionRTT = new BABYLON.RenderTargetTexture(name + "_refraction", { width: renderTargetSize.x, height: renderTargetSize.y }, scene, false, true);
            this._reflectionRTT = new BABYLON.RenderTargetTexture(name + "_reflection", { width: renderTargetSize.x, height: renderTargetSize.y }, scene, false, true);
            scene.customRenderTargets.push(this._refractionRTT);
            scene.customRenderTargets.push(this._reflectionRTT);
            var isVisible;
            var clipPlane = null;
            var savedViewMatrix;
            var mirrorMatrix = BABYLON.Matrix.Zero();
            this._refractionRTT.onBeforeRender = function () {
                if (_this._mesh) {
                    isVisible = _this._mesh.isVisible;
                    _this._mesh.isVisible = false;
                }
                // Clip plane
                clipPlane = scene.clipPlane;
                var positiony = _this._mesh ? _this._mesh.position.y : 0.0;
                scene.clipPlane = BABYLON.Plane.FromPositionAndNormal(new BABYLON.Vector3(0, positiony + 0.05, 0), new BABYLON.Vector3(0, 1, 0));
            };
            this._refractionRTT.onAfterRender = function () {
                if (_this._mesh) {
                    _this._mesh.isVisible = isVisible;
                }
                // Clip plane
                scene.clipPlane = clipPlane;
            };
            this._reflectionRTT.onBeforeRender = function () {
                if (_this._mesh) {
                    isVisible = _this._mesh.isVisible;
                    _this._mesh.isVisible = false;
                }
                // Clip plane
                clipPlane = scene.clipPlane;
                var positiony = _this._mesh ? _this._mesh.position.y : 0.0;
                scene.clipPlane = BABYLON.Plane.FromPositionAndNormal(new BABYLON.Vector3(0, positiony - 0.05, 0), new BABYLON.Vector3(0, -1, 0));
                // Transform
                BABYLON.Matrix.ReflectionToRef(scene.clipPlane, mirrorMatrix);
                savedViewMatrix = scene.getViewMatrix();
                mirrorMatrix.multiplyToRef(savedViewMatrix, _this._reflectionTransform);
                scene.setTransformMatrix(_this._reflectionTransform, scene.getProjectionMatrix());
                scene.getEngine().cullBackFaces = false;
                scene._mirroredCameraPosition = BABYLON.Vector3.TransformCoordinates(scene.activeCamera.position, mirrorMatrix);
            };
            this._reflectionRTT.onAfterRender = function () {
                if (_this._mesh) {
                    _this._mesh.isVisible = isVisible;
                }
                // Clip plane
                scene.clipPlane = clipPlane;
                // Transform
                scene.setTransformMatrix(savedViewMatrix, scene.getProjectionMatrix());
                scene.getEngine().cullBackFaces = true;
                scene._mirroredCameraPosition = null;
            };
        };
        WaterMaterial.prototype.getAnimatables = function () {
            var results = [];
            if (this.bumpTexture && this.bumpTexture.animations && this.bumpTexture.animations.length > 0) {
                results.push(this.bumpTexture);
            }
            if (this._reflectionRTT && this._reflectionRTT.animations && this._reflectionRTT.animations.length > 0) {
                results.push(this._reflectionRTT);
            }
            if (this._refractionRTT && this._refractionRTT.animations && this._refractionRTT.animations.length > 0) {
                results.push(this._refractionRTT);
            }
            return results;
        };
        WaterMaterial.prototype.dispose = function (forceDisposeEffect) {
            if (this.bumpTexture) {
                this.bumpTexture.dispose();
            }
            var index = this.getScene().customRenderTargets.indexOf(this._refractionRTT);
            if (index != -1) {
                this.getScene().customRenderTargets.splice(index, 1);
            }
            index = -1;
            index = this.getScene().customRenderTargets.indexOf(this._reflectionRTT);
            if (index != -1) {
                this.getScene().customRenderTargets.splice(index, 1);
            }
            if (this._reflectionRTT) {
                this._reflectionRTT.dispose();
            }
            if (this._refractionRTT) {
                this._refractionRTT.dispose();
            }
            _super.prototype.dispose.call(this, forceDisposeEffect);
        };
        WaterMaterial.prototype.clone = function (name) {
            var _this = this;
            return BABYLON.SerializationHelper.Clone(function () { return new WaterMaterial(name, _this.getScene()); }, this);
        };
        WaterMaterial.prototype.serialize = function () {
            var serializationObject = BABYLON.SerializationHelper.Serialize(this);
            serializationObject.customType = "BABYLON.WaterMaterial";
            return serializationObject;
        };
        // Statics
        WaterMaterial.Parse = function (source, scene, rootUrl) {
            return BABYLON.SerializationHelper.Parse(function () { return new WaterMaterial(source.name, scene); }, source, scene, rootUrl);
        };
        WaterMaterial.CreateDefaultMesh = function (name, scene) {
            var mesh = BABYLON.Mesh.CreateGround(name, 512, 512, 32, scene, false);
            return mesh;
        };
        __decorate([
            BABYLON.serializeAsTexture()
        ], WaterMaterial.prototype, "bumpTexture", void 0);
        __decorate([
            BABYLON.serializeAsColor3()
        ], WaterMaterial.prototype, "diffuseColor", void 0);
        __decorate([
            BABYLON.serializeAsColor3()
        ], WaterMaterial.prototype, "specularColor", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "specularPower", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "disableLighting", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "windForce", void 0);
        __decorate([
            BABYLON.serializeAsVector2()
        ], WaterMaterial.prototype, "windDirection", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "waveHeight", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "bumpHeight", void 0);
        __decorate([
            BABYLON.serializeAsColor3()
        ], WaterMaterial.prototype, "waterColor", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "colorBlendFactor", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "waveLength", void 0);
        __decorate([
            BABYLON.serialize()
        ], WaterMaterial.prototype, "waveSpeed", void 0);
        return WaterMaterial;
    }(BABYLON.Material));
    BABYLON.WaterMaterial = WaterMaterial;
})(BABYLON || (BABYLON = {}));
