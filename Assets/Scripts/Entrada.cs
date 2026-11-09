using UnityEngine;

//Lectura del teclado para Ciguín. Las pruebas automáticas pueden simular las teclas con Simular().
public static class Entrada {

	static bool simulando;
	static float ejeSimulado;
	static bool saltoSimulado;
	static bool saltoAbajoSimulado;

	public static void Simular(float eje, bool salto) {
		simulando = true;
		ejeSimulado = eje;
		if (salto && !saltoSimulado) saltoAbajoSimulado = true;
		saltoSimulado = salto;
	}

	public static void DejarDeSimular() => simulando = false;

	public static float Horizontal {
		get {
			if (simulando) return ejeSimulado;
			float x = 0f;
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x -= 1f;
			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x += 1f;
			return x;
		}
	}

	public static bool Correr => !simulando && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.X));

	public static bool SaltoAbajo {
		get {
			if (simulando) {
				bool s = saltoAbajoSimulado;
				saltoAbajoSimulado = false;
				return s;
			}
			return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) ||
				Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z);
		}
	}

	public static bool SaltoMantenido {
		get {
			if (simulando) return saltoSimulado;
			return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) ||
				Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z);
		}
	}
}
