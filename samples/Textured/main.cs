﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Glfw;
using vke;
using Vulkan;
using Image = vke.Image;

namespace Textured {
	/// <summary>
	/// Simple textured quad sample
	/// </summary>
	class Program : SampleBase {
		static void Main (string[] args) {
#if DEBUG
			Instance.VALIDATION = true;
#endif
			using (Program vke = new Program ()) {
				vke.Run ();
			}
		}
		protected override void configureEnabledFeatures (VkPhysicalDeviceFeatures available_features, ref VkPhysicalDeviceFeatures enabled_features) {
			base.configureEnabledFeatures (available_features, ref enabled_features);
			enabled_features.textureCompressionBC = available_features.textureCompressionBC;
			enabled_features.textureCompressionASTC_LDR = available_features.textureCompressionASTC_LDR;
		}

		float rotSpeed = 0.01f, zoomSpeed = 0.01f;
		float rotX, rotY, rotZ = 0f, zoom = 1f;

		struct Matrices {
			public Matrix4x4 projection;
			public Matrix4x4 view;
			public Matrix4x4 model;
		}

		Matrices matrices;

		HostBuffer uboMats;
		GPUBuffer<float> vbo;
		GPUBuffer<ushort> ibo;

		DescriptorPool descriptorPool;
		DescriptorSetLayout dsLayout;
		DescriptorSet descriptorSet;

		GraphicPipeline pipeline;
		FrameBuffers frameBuffers;

		Image texture;
		Image nextTexture;

		float[] vertices = {
			 1.0f,  1.0f, 0.0f ,  1.0f, 0.0f,
			-1.0f,  1.0f, 0.0f ,  0.0f, 0.0f,
			-1.0f, -1.0f, 0.0f ,  0.0f, 1.0f,
			 1.0f, -1.0f, 0.0f ,  1.0f, 1.0f,
		};
		ushort[] indices = { 0, 1, 2, 2, 0, 3 };
		int currentImgIndex = 0;
		string[] imgPathes = {
			vke.samples.Utils.GetDataFile ("textures/texspace256.jpg"),
			vke.samples.Utils.GetDataFile ("textures/texturearray_rocks_bc3_unorm.ktx"),
			vke.samples.Utils.GetDataFile ("textures/texture.jpg"),
			vke.samples.Utils.GetDataFile ("textures/tex256.jpg"),
			vke.samples.Utils.GetDataFile ("font.ktx"),
		};

		protected override void initVulkan () {
			base.initVulkan ();

			cmds = cmdPool.AllocateCommandBuffer(swapChain.ImageCount);

			loadTexture (imgPathes[currentImgIndex]);

			vbo = new GPUBuffer<float> (presentQueue, cmdPool, VkBufferUsageFlags.VertexBuffer, vertices);
			ibo = new GPUBuffer<ushort> (presentQueue, cmdPool, VkBufferUsageFlags.IndexBuffer, indices);

			descriptorPool = new DescriptorPool (dev, 1,
				new VkDescriptorPoolSize (VkDescriptorType.UniformBuffer),
				new VkDescriptorPoolSize (VkDescriptorType.CombinedImageSampler)
			);

			dsLayout = new DescriptorSetLayout (dev, 0,
				new VkDescriptorSetLayoutBinding (0, VkShaderStageFlags.Vertex, VkDescriptorType.UniformBuffer),
				new VkDescriptorSetLayoutBinding (1, VkShaderStageFlags.Fragment, VkDescriptorType.CombinedImageSampler));

			using (GraphicPipelineConfig cfg = GraphicPipelineConfig.CreateDefault (VkPrimitiveTopology.TriangleList, VkSampleCountFlags.SampleCount4)) {

				cfg.Layout = new PipelineLayout (dev, dsLayout);
				cfg.RenderPass = new RenderPass (dev, swapChain.ColorFormat, dev.GetSuitableDepthFormat (), cfg.Samples);

				cfg.AddVertexBinding (0, 5 * sizeof (float));
				cfg.AddVertexAttributes (0, VkFormat.R32g32b32Sfloat, VkFormat.R32g32Sfloat);

				cfg.AddShader (dev, VkShaderStageFlags.Vertex, "#shaders.main.vert.spv");
				cfg.AddShader (dev, VkShaderStageFlags.Fragment, "#shaders.main.frag.spv");

				pipeline = new GraphicPipeline (cfg);
			}


			uboMats = new HostBuffer (dev, VkBufferUsageFlags.UniformBuffer, matrices);
			uboMats.Map ();//permanent map

			descriptorSet = descriptorPool.Allocate (dsLayout);

			updateTextureSet ();

			DescriptorSetWrites uboUpdate = new DescriptorSetWrites (descriptorSet, dsLayout.Bindings[0]);
			uboUpdate.Write (dev, uboMats.Descriptor);
		}

		void buildCommandBuffers () {
			dev.WaitIdle ();
			cmdPool.Reset();
			for (int i = 0; i < swapChain.ImageCount; ++i) {
				PrimaryCommandBuffer cmd = cmds[i];
				FrameBuffer fb = frameBuffers[i];

				cmd.Start();

				pipeline.RenderPass.Begin (cmd, fb);

				cmd.SetViewport (fb.Width, fb.Height);
				cmd.SetScissor (fb.Width, fb.Height);
				cmd.BindDescriptorSet (pipeline.Layout, descriptorSet);

				pipeline.Bind (cmd);

				cmd.BindVertexBuffer (vbo, 0);
				cmd.BindIndexBuffer (ibo, VkIndexType.Uint16);
				cmd.DrawIndexed ((uint)indices.Length);

				pipeline.RenderPass.End (cmd);

				cmd.End ();
			}
		}


		VkMemoryPropertyFlags imgProp = VkMemoryPropertyFlags.DeviceLocal;
		bool genMipMaps = true;
		VkImageTiling tiling = VkImageTiling.Optimal;

		//in the thread of the keyboard
		void loadTexture (string path) {
			try {
				Console.WriteLine ($"Loading:{path}");
				if (path.EndsWith ("ktx", StringComparison.OrdinalIgnoreCase))
					nextTexture = KTX.KTX.Load (presentQueue, cmdPool, path,
						VkImageUsageFlags.Sampled, imgProp, genMipMaps, tiling);
				else
 					nextTexture = Image.Load (presentQueue, cmdPool, path, VkFormat.R8g8b8a8Unorm, imgProp, tiling, genMipMaps);
				updateViewRequested = true;
			} catch (Exception ex) {
				Console.WriteLine (ex);
				nextTexture = null;
			}
		}

		//in the main vulkan thread
		void updateTextureSet (){
			nextTexture.CreateView ();
			nextTexture.CreateSampler ();
			nextTexture.Descriptor.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;

			dev.WaitIdle ();

			DescriptorSetWrites uboUpdate = new DescriptorSetWrites (descriptorSet, dsLayout.Bindings[1]);
			uboUpdate.Write (dev, nextTexture.Descriptor);

			texture?.Dispose ();
			texture = nextTexture;
			nextTexture = null;
		}

		void updateMatrices () {
			matrices.projection = Matrix4x4.CreatePerspectiveFieldOfView (Utils.DegreesToRadians (60f), (float)swapChain.Width / (float)swapChain.Height, 0.1f, 256.0f);
			matrices.view = Matrix4x4.CreateTranslation (0, 0, -2.5f * zoom);
			matrices.model =
				Matrix4x4.CreateFromAxisAngle (Vector3.UnitZ, rotZ) *
				Matrix4x4.CreateFromAxisAngle (Vector3.UnitY, rotY) *
				Matrix4x4.CreateFromAxisAngle (Vector3.UnitX, rotX);

			uboMats.Update (matrices, (uint)Marshal.SizeOf<Matrices> ());
		}

		public override void UpdateView () {
			if (nextTexture != null) {
				updateTextureSet ();
				buildCommandBuffers ();
			}else
				updateMatrices ();

			updateViewRequested = false;
		}

		protected override void onMouseMove (double xPos, double yPos) {
			double diffX = lastMouseX - xPos;
			double diffY = lastMouseY - yPos;
			if (GetButton (MouseButton.Left) == InputAction.Press) {
				rotY -= rotSpeed * (float)diffX;
				rotX += rotSpeed * (float)diffY;
				updateViewRequested = true;
			} else if (GetButton (MouseButton.Right) == InputAction.Press) {
				zoom += zoomSpeed * (float)diffY;
				updateViewRequested = true;
			}
		}

		protected override void onKeyDown (Key key, int scanCode, Modifier modifiers) {
			switch (key) {
				case Key.Space:
					currentImgIndex++;
					if (currentImgIndex == imgPathes.Length)
						currentImgIndex = 0;
					loadTexture (imgPathes[currentImgIndex]);
					break;
				default:
					base.onKeyDown (key, scanCode, modifiers);
					break;
			}
		}

		protected override void OnResize () {
			base.OnResize();

			updateMatrices ();

			frameBuffers?.Dispose();
			frameBuffers = pipeline.RenderPass.CreateFrameBuffers(swapChain);

			buildCommandBuffers ();
		}

		protected override void Dispose (bool disposing) {
			dev.WaitIdle ();

			if (disposing) {
				if (!isDisposed) {
					pipeline.Dispose ();
					dsLayout.Dispose ();
					frameBuffers.Dispose();
					descriptorPool.Dispose ();
					texture?.Dispose ();
					nextTexture?.Dispose ();
					vbo.Dispose ();
					ibo.Dispose ();
					uboMats.Dispose ();
				}
			}

			base.Dispose (disposing);
		}
	}
}
