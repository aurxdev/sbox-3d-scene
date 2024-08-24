using Sandbox;
using Sandbox.Citizen;

public sealed class Player : Component
{
	/// <summary>
	/// The player's camera.
	/// </summary>
	[Property]
	[Category("Components")]
	public GameObject Camera { get; set; }
	[Property] 
	[Category("Components")]
	public CharacterController Controller { get; set; }
	[Property]
	[Category("Components")]
	public CitizenAnimationHelper Animator { get; set; }
	[Property]
	[Category("Stats")]
	public float WalkSpeed { get; set; } = 120.0f;
	[Property] 
	[Category("Stats")]
	public float RunSpeed { get; set; } = 320.0f;
	[Property]
	[Category("Stats")]
	public float JumpStrenght { get; set; } = 400.0f;

	/// <summary>
	/// Where the camera rotates around the player.
	/// </summary>
	[Property]
	public Vector3 EyePosition { get; set; }

	public Angles EyeAngles { get; set; }
	Transform _initialCameraTransform;

	
	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch(MathX.Clamp(EyeAngles.pitch, -80f, 80f));
		Transform.Rotation = Rotation.FromYaw(EyeAngles.yaw);

		if (Camera != null)
		{
			Camera.Transform.Local = _initialCameraTransform.RotateAround(EyePosition, EyeAngles.WithYaw(0f));
		}

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if (Controller == null)
			return;
		var speed = Input.Down("Run") ? RunSpeed : WalkSpeed;
		var velocity = Input.AnalogMove.Normal * speed * Transform.Rotation;
		Controller.Accelerate(velocity);
		if(Controller.IsOnGround){

			Controller.Acceleration = 10f;
			Controller.ApplyFriction(5f);

			if(Input.Pressed("Jump")){
				Controller.Punch(Vector3.Up * JumpStrenght);

				if (Animator != null)
				{
					Animator.TriggerJump();
				}

			}

		}
		else{
			Controller.Acceleration = 5f;
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}
		Controller.Move();

		if (Animator != null)
		{
			Animator.IsGrounded = Controller.IsOnGround;
			Animator.WithVelocity(Controller.Velocity);
		}

	}

	protected override void OnStart()
	{
		if (Camera == null)
			return;
		_initialCameraTransform = Camera.Transform.Local;

		if (Components.TryGet<SkinnedModelRenderer>(out var model))
		{
			var clothing = ClothingContainer.CreateFromLocalUser();
			clothing.Apply(model);
		}

	}


	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineSphere(EyePosition,10f);
	}
}
