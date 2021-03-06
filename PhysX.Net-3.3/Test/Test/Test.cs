﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhysX.Test
{
	[TestClass]
	public abstract class Test
	{
		private ErrorLog _errorLog;

		private static PhysX.Foundation _foundation;
		private static PhysX.Physics _physics;

		[TestCleanup]
		public void CleanUp()
		{
			if (Physics.Instantiated)
			{
				_foundation.Dispose();
				_foundation = null;

				throw new Exception("After a test has run, the Physics singleton should have been disposed");
			}

			ObjectTable.Clear();

			if (_errorLog != null && _errorLog.HasErrors)
			{
				Trace.TraceError("There were errors, check the error log");
			}

			_errorLog = null;
		}

		[AssemblyInitialize]
		public static void AssemblyInit(TestContext ctx)
		{
			TestDependantFiles.CopyDependantFiles(ctx);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			Trace.WriteLine(GetCurrentMethod());

			if (Physics.Instantiated)
				throw new Exception("Before a test run, the Physics singleton should not be initalized. Probably spill over from a previous test.");
		}

		private Physics CreatePhysics(ErrorCallback errorCallback = null)
		{
			if (Physics.Instantiated)
				Assert.Fail("Physics is still created");

			if (errorCallback == null)
				errorCallback = _errorLog = new ErrorLog();

			if (_physics != null)
			{
				_physics.Dispose();
				_physics = null;
			}
			if (_foundation != null)
			{
				_foundation.Dispose();
				_foundation = null;
			}

			_foundation = new Foundation(errorCallback);

			_physics = new Physics(_foundation, checkRuntimeFiles: true);

			return _physics;
		}
		protected PhysicsAndSceneTestUnit CreatePhysicsAndScene()
		{
			_errorLog = new ErrorLog();

			var physics = CreatePhysics(_errorLog);

			var scene = physics.CreateScene();

			return new PhysicsAndSceneTestUnit()
			{
				Foundation = physics.Foundation,
				Physics = scene.Physics,
				Scene = scene,
				ErrorOutput = _errorLog
			};
		}

		protected RigidDynamic CreateBoxActor(Scene scene, float x, float y, float z)
		{
			return CreateBoxActor(scene, new Vector3(5, 5, 5), new Vector3(x, y, z));
		}
		protected RigidDynamic CreateBoxActor(Scene scene, Vector3 size, Vector3 position)
		{
			var material = scene.Physics.CreateMaterial(0.5f, 0.5f, 0.1f);

			var rigid = scene.Physics.CreateRigidDynamic();

			var shape = rigid.CreateShape(new BoxGeometry(size / 2), material);

			rigid.GlobalPose = Matrix4x4.CreateTranslation(position);

			scene.AddActor(rigid);

			return rigid;
		}

		protected void AssertNoPhysXErrors(PhysicsAndSceneTestUnit physics)
		{
			Assert.IsFalse(physics.ErrorOutput.HasErrors, physics.ErrorOutput.LastError);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public string GetCurrentMethod()
		{
			StackTrace st = new StackTrace();
			StackFrame sf = st.GetFrame(1);

			return sf.GetMethod().Name;
		}
	}
}