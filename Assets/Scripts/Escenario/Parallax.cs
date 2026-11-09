using UnityEngine;

//Fondos que se mueven más lento que la cámara para dar sensación de profundidad
public class Parallax : MonoBehaviour {

	[Range(0f, 1f)] public float factor = 0.5f;
	public float desplazamientoAuto;
	Transform camara;
	float inicioX;
	float auto;

	void Start() {
		camara = Camera.main.transform;
		inicioX = transform.position.x;
	}

	void LateUpdate() {
		auto += desplazamientoAuto * Time.deltaTime;
		var p = transform.position;
		p.x = inicioX + camara.position.x * factor + auto;
		transform.position = p;
	}
}
