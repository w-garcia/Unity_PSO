using UnityEngine;
using System.Collections;

[System.Serializable]
public class Boundary
{
	public static float MaxX = 55;
	public static float MaxY = 105;
	public static float MaxZ = 50;
}

public class Generator : MonoBehaviour 
{
	public SwarmBrain swarmBrain = new SwarmBrain();

	public GameObject particleTemplate;

	public GameObject optimaTemplate;

	public GameObject OptimaLightTemplate;

	public int numParticles;

	public int numOptima;

	public void destroyObject()
	{
		Destroy(particleTemplate);
	}

	// Use this for initialization
	void Start () 
	{

		InitializeOptimaObjects();

		InitializeParticleObjects();

		swarmBrain.InitializeParticleVelocities();
	}

	private void InitializeParticleObjects()
	{
		for (int i = 0; i < numParticles; i++)
		{

			float randX, randY, randZ;

            SwarmUtil.GetRandomXYZWithPositiveY(out randX, out randY, out randZ);

			Vector3 position = new Vector3(randX, randY, randZ);

			GameObject goInstance = (GameObject)Instantiate(particleTemplate, position, transform.rotation);

			Particle temp = new Particle(goInstance);
			temp.bestPosition = goInstance.GetComponent<Rigidbody>().position;
			
			swarmBrain.particles.Add(temp);
		}
	}

	private void InitializeOptimaObjects()
	{
		for (int i = 0; i < numOptima; i++)
		{
			float randX, randY, randZ;

            SwarmUtil.GetRandomXYZWithPositiveY(out randX, out randY, out randZ);

			Vector3 position = new Vector3(randX, randY, randZ);

			GameObject goInstance = (GameObject)Instantiate(optimaTemplate, position, transform.rotation);

			Particle temp = new Particle(goInstance);


			GameObject pointLight = (GameObject)Instantiate(OptimaLightTemplate, position, transform.rotation);
            pointLight.transform.parent = temp.gameObject.transform;


            swarmBrain.optimaList.Add(temp);


		}
	}



	// Update is called once per frame
	void Update () 
	{
		swarmBrain.UpdateSwarmFitness();
	
	}

	void FixedUpdate()
	{
		swarmBrain.UpdateParticleVelocities();
	}

	


}

class Particle
{
	public GameObject gameObject;

	public Vector3 bestPosition;

	public float bestPositionFitness = Mathf.Infinity;

	public Particle(GameObject obj)
	{
		gameObject = obj;
	}
}

[System.Serializable]
public class SwarmBrain
{
	public float particleSpeed;

    public float optimaSpeed;

	public ArrayList optimaList = new ArrayList();

	private Vector3 globalBestPosition = new Vector3(10000, 10000, 10000);

	private float globalBestPositionFitness = Mathf.Infinity;

	public ArrayList particles = new ArrayList();

	public float personalBestInfluence; 

	public float globalBestInfluence;

    public float entropyPercentage;

	public SwarmBrain()
	{

	}



	public void InitializeParticleVelocities()
	{
		foreach (Particle particle in particles)
		{
			Rigidbody rb = particle.gameObject.GetComponent<Rigidbody>();

			float randX;
			float randY;
			float randZ;
			SwarmUtil.GetRandomXYZ(out randX, out randY, out randZ);

            rb.AddForce(new Vector3(randX, randY, randZ).normalized * particleSpeed);
		}

        foreach (Particle optima in optimaList)
        {
            Rigidbody rb = optima.gameObject.GetComponent<Rigidbody>();

            float randX;
            float randY;
            float randZ;
            SwarmUtil.GetRandomXYZ(out randX, out randY, out randZ);

            rb.AddForce(new Vector3(randX, randY, randZ).normalized * optimaSpeed);
        }
	}

	public void UpdateParticleVelocities()
	{
        entropyPercentage = Mathf.Clamp(entropyPercentage, 0, 1);

		foreach (Particle particle in particles)
		{
			Rigidbody rb = particle.gameObject.GetComponent<Rigidbody>();

			float randXp, randXg;
			float randYp, randYg;
			float randZp, randZg;
            SwarmUtil.GetRandomUnitSphereXYZ(out randXp, out randYp, out randZp, entropyPercentage);
            SwarmUtil.GetRandomUnitSphereXYZ(out randXg, out randYg, out randZg, entropyPercentage);

			float XpMotive = randXp * (particle.bestPosition.x - rb.position.x);
			float XgMotive = randXg * (globalBestPosition.x - rb.position.x);

			float YpMotive = randYp * (particle.bestPosition.y - rb.position.y);
			float YgMotive = randYg * (globalBestPosition.y - rb.position.y);

			float ZpMotive = randZp * (particle.bestPosition.z - rb.position.z);
			float ZgMotive = randZg * (globalBestPosition.z - rb.position.z);

            float X = personalBestInfluence * XpMotive + globalBestInfluence * XgMotive;
            float Y = personalBestInfluence * YpMotive + globalBestInfluence * YgMotive;
            float Z = personalBestInfluence * ZpMotive + globalBestInfluence * ZgMotive;

            //Vector3 influence = new Vector3(X * particleSpeed, Y * particleSpeed, Z * particleSpeed);
			rb.AddForce(new Vector3(X, Y, Z).normalized * particleSpeed);
		}

        foreach (Particle optima in optimaList)
        {
            Rigidbody rb = optima.gameObject.GetComponent<Rigidbody>();

            float randX;
            float randY;
            float randZ;
            SwarmUtil.GetRandomXYZ(out randX, out randY, out randZ);

            rb.AddForce(new Vector3(randX, randY, randZ).normalized * optimaSpeed);
        }
	}

	internal void UpdateSwarmFitness()
	{
		foreach(Particle optima in optimaList)
		{
			foreach (Particle particle in particles)
			{
				Rigidbody rb = particle.gameObject.GetComponent<Rigidbody>();

				float distance = Vector3.Distance(rb.position, optima.gameObject.GetComponent<Rigidbody>().position);

				if (distance < particle.bestPositionFitness)
				{
					particle.bestPosition = rb.position;
					particle.bestPositionFitness = distance;

					if (distance < globalBestPositionFitness)
					{
						globalBestPosition = rb.position;
						globalBestPositionFitness = distance;
					}
				}

			}
		}
	}
}

static class SwarmUtil
{
	public static void GetRandomXandZ(out float randX, out float randZ, float maxX, float maxZ)
	{
		randX = Random.value * maxX;
		randZ = Random.value * maxZ;

		if (Random.value > 0.5)
		{
			randX *= -1;
		}
		if (Random.value > 0.5)
		{
			randZ *= -1;
		}
	}

	public static void GetRandomXYZ(out float randX, out float randY, out float randZ)
	{
		randX = Random.value * Boundary.MaxX;
		randY = Random.value * Boundary.MaxY;
		randZ = Random.value * Boundary.MaxZ;

		if (Random.value > 0.5)
		{
			randX *= -1;
		}
		if (Random.value > 0.5)
		{
			randZ *= -1;
		}
        if (Random.value > 0.5)
        {
            randY *= -1;
        }
	}

    public static void GetRandomXYZWithPositiveY(out float randX, out float randY, out float randZ)
    {
        randX = Random.value * Boundary.MaxX;
        randY = Random.value * Boundary.MaxY;
        randZ = Random.value * Boundary.MaxZ;

        if (Random.value > 0.5)
        {
            randX *= -1;
        }
        if (Random.value > 0.5)
        {
            randZ *= -1;
        }
    }

    public static void GetRandomUnitSphereXYZ(out float randX, out float randY, out float randZ, float entropy)
    {
        randX = Random.value;
        randY = Random.value;
        randZ = Random.value;

        if (Random.value > 1 - entropy)
        {
            randX *= -1;
        }
        if (Random.value > 1 - entropy)
        {
            randZ *= -1;
        }
        if (Random.value > 1 - entropy)
        {
            randY *= -1;
        }
    }
}
