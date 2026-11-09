using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//Sonidos y música del juego, sintetizados aquí al estilo chiptune (melodías originales).
public static partial class ConstructorProyecto {

	const int FRECUENCIA = 22050;

	static AudioClip mMenu, mNivel1, mNivel2, mNivel3, mJefe, mVictoria;

	// ------------------------------------------------ WAV

	static void EscribirWav(string ruta, float[] datos) {
		float pico = datos.Max(m => Mathf.Abs(m));
		float escala = pico > 0f ? 0.9f / pico : 1f;
		using (var bw = new BinaryWriter(File.Create(ruta))) {
			int bytesDatos = datos.Length * 2;
			bw.Write("RIFF".ToCharArray());
			bw.Write(36 + bytesDatos);
			bw.Write("WAVE".ToCharArray());
			bw.Write("fmt ".ToCharArray());
			bw.Write(16);
			bw.Write((short)1);
			bw.Write((short)1);
			bw.Write(FRECUENCIA);
			bw.Write(FRECUENCIA * 2);
			bw.Write((short)2);
			bw.Write((short)16);
			bw.Write("data".ToCharArray());
			bw.Write(bytesDatos);
			foreach (float m in datos) bw.Write((short)(Mathf.Clamp(m * escala, -1f, 1f) * short.MaxValue));
		}
	}

	static AudioClip GuardarClip(string nombre, float[] datos, bool musica) {
		string ruta = "Assets/Audio/" + nombre + ".wav";
		EscribirWav(ruta, datos);
		AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
		var imp = (AudioImporter)AssetImporter.GetAtPath(ruta);
		var conf = imp.defaultSampleSettings;
		conf.loadType = musica ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
		conf.compressionFormat = AudioCompressionFormat.Vorbis;
		conf.quality = musica ? 0.6f : 0.8f;
		imp.defaultSampleSettings = conf;
		imp.SaveAndReimport();
		return AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
	}

	static AudioClip Efecto(string nombre, float segundos, Func<float, System.Random, float> muestra) {
		var r = new System.Random(nombre.Length * 7919);
		int n = (int)(segundos * FRECUENCIA);
		var datos = new float[n];
		for (int i = 0; i < n; i++) datos[i] = muestra(i / (float)FRECUENCIA, r);
		//Pequeño fundido para evitar clics
		int f = Mathf.Min(200, n / 4);
		for (int i = 0; i < f; i++) datos[n - 1 - i] *= i / (float)f;
		return GuardarClip(nombre, datos, false);
	}

	static float Seno(float frecuencia, float t) => Mathf.Sin(2f * Mathf.PI * frecuencia * t);
	static float Ruido(System.Random r) => (float)(r.NextDouble() * 2.0 - 1.0);
	static float Pulso(float fase, float ciclo) => (fase - Mathf.Floor(fase)) < ciclo ? 1f : -1f;
	static float Triangulo(float fase) => 4f * Mathf.Abs(fase - Mathf.Floor(fase) - 0.5f) - 1f;
	static float Midi(int nota) => 440f * Mathf.Pow(2f, (nota - 69) / 12f);

	// ------------------------------------------------ Efectos de sonido

	static void CrearEfectos(Juego j) {

		//Fases acumuladas para barridos de tono sin saltos
		float fase = 0f;
		float Barrido(float f, float t) { fase += f / FRECUENCIA; return fase; }

		fase = 0f;
		j.sSalto = Efecto("Salto", 0.2f, (t, r) => {
			float f = Mathf.Lerp(260f, 820f, Mathf.Pow(t / 0.2f, 0.6f));
			return Pulso(Barrido(f, t), 0.25f) * 0.5f * (1f - t / 0.2f);
		});

		j.sLarimar = Efecto("Larimar", 0.45f, (t, r) => {
			float f = t < 0.07f ? Midi(83) : Midi(88);
			float env = t < 0.07f ? 1f : Mathf.Exp(-(t - 0.07f) * 7f);
			return Pulso(f * t, 0.5f) * 0.4f * env;
		});

		fase = 0f;
		j.sAplastar = Efecto("Aplastar", 0.22f, (t, r) => {
			float f = Mathf.Lerp(420f, 90f, t / 0.22f);
			return (Pulso(Barrido(f, t), 0.5f) * 0.5f + Ruido(r) * 0.25f) * (1f - t / 0.22f);
		});

		fase = 0f;
		j.sDano = Efecto("Dano", 0.55f, (t, r) => {
			float f = Mathf.Lerp(700f, 120f, t / 0.55f) * (1f + 0.15f * Seno(30f, t));
			return Pulso(Barrido(f, t), 0.5f) * 0.45f * (1f - t / 0.55f);
		});

		j.sBloque = Efecto("Bloque", 0.14f, (t, r) => {
			float f = Mathf.Lerp(180f, 60f, t / 0.14f);
			return (Seno(f, t) * 0.8f + Ruido(r) * 0.2f) * Mathf.Exp(-t * 25f);
		});

		float lp = 0f;
		j.sRomper = Efecto("Romper", 0.45f, (t, r) => {
			lp += 0.35f * (Ruido(r) - lp);
			float crujido = (Seno(90f, t) * 0.4f + lp) * Mathf.Exp(-t * 7f);
			return crujido + (t < 0.05f ? Ruido(r) * 0.5f : 0f);
		});

		fase = 0f;
		j.sAparece = Efecto("Aparece", 0.6f, (t, r) => {
			float f = Mathf.Lerp(200f, 700f, t / 0.6f) * (1f + 0.08f * Seno(18f, t));
			return Triangulo(Barrido(f, t)) * 0.6f;
		});

		int[] notasMango = { 72, 76, 79, 84, 88 };
		j.sMango = Efecto("Mango", 0.6f, (t, r) => {
			int i = Mathf.Min((int)(t / 0.08f), notasMango.Length - 1);
			return Pulso(Midi(notasMango[i]) * t, 0.25f) * 0.4f * (t > 0.4f ? (0.6f - t) / 0.2f : 1f);
		});

		int[] notasVida = { 76, 79, 88, 84, 86, 91 };
		j.sVidaExtra = Efecto("VidaExtra", 0.75f, (t, r) => {
			int i = Mathf.Min((int)(t / 0.1f), notasVida.Length - 1);
			return Pulso(Midi(notasVida[i]) * t, 0.5f) * 0.35f * Mathf.Min(1f, (0.75f - t) * 5f);
		});

		fase = 0f;
		j.sDisparo = Efecto("Disparo", 0.15f, (t, r) => {
			float f = Mathf.Lerp(1200f, 400f, t / 0.15f);
			return Pulso(Barrido(f, t), 0.5f) * 0.35f * (1f - t / 0.15f);
		});

		lp = 0f;
		j.sFuego = Efecto("Fuego", 0.5f, (t, r) => {
			lp += 0.12f * (Ruido(r) - lp);
			return lp * 2.5f * Mathf.Sin(Mathf.PI * t / 0.5f) + Seno(Mathf.Lerp(300f, 100f, t / 0.5f), t) * 0.2f * (1f - t / 0.5f);
		});

		fase = 0f;
		j.sGolpeJefe = Efecto("GolpeJefe", 0.5f, (t, r) => {
			float f = Mathf.Lerp(500f, 50f, t / 0.5f);
			return (Pulso(Barrido(f, t), 0.5f) * 0.5f + Ruido(r) * 0.3f * Mathf.Exp(-t * 10f)) * (1f - t / 0.5f);
		});

		lp = 0f;
		j.sRugido = Efecto("Rugido", 1.3f, (t, r) => {
			lp += 0.08f * (Ruido(r) - lp);
			float f = 70f + 25f * Seno(3f, t);
			float env = Mathf.Min(1f, t * 8f) * Mathf.Clamp01((1.3f - t) * 2f);
			return (Pulso(f * t, 0.3f) * 0.35f + lp * 3f) * env * (0.7f + 0.3f * Seno(23f, t));
		});

		lp = 0f;
		j.sTemblor = Efecto("Temblor", 0.8f, (t, r) => {
			lp += 0.05f * (Ruido(r) - lp);
			return (Seno(45f, t) * 0.6f + lp * 4f) * Mathf.Exp(-t * 4f);
		});

		int[] notasCheck = { 79, 84, 88 };
		j.sCheckpoint = Efecto("PuntoControl", 0.5f, (t, r) => {
			int i = Mathf.Min((int)(t / 0.09f), 2);
			return (Pulso(Midi(notasCheck[i]) * t, 0.25f) * 0.3f + Seno(Midi(notasCheck[i]) * 2f, t) * 0.2f) * Mathf.Exp(-t * 3f);
		});

		j.sPausa = Efecto("Pausa", 0.25f, (t, r) => {
			float f = t < 0.08f ? Midi(81) : t < 0.16f ? Midi(76) : Midi(81);
			return Pulso(f * t, 0.5f) * 0.3f;
		});

		fase = 0f;
		j.sCaida = Efecto("Caida", 0.4f, (t, r) => {
			float f = Mathf.Lerp(1400f, 500f, t / 0.4f);
			return Seno(f, t) * 0.25f * (1f - t / 0.4f) + Triangulo(Barrido(f * 0.5f, t)) * 0.15f;
		});

		j.sBoton = Efecto("Boton", 0.1f, (t, r) => Pulso(Midi(t < 0.04f ? 84 : 91) * t, 0.5f) * 0.3f * (1f - t / 0.1f));

		//Jingles hechos con el secuenciador
		j.sMuerte = Secuenciar("Muerte", 170f, new[] { "Dm", "A", "Dm" },
			"A4 = = D5 = = C#5 = C5 = B4 = Bb4 = = = A4 = = = = = = -", "R=======", "", false, false);
		j.sNivelCompletado = Secuenciar("NivelCompletado", 150f, new[] { "C", "G", "C" },
			"G4 C5 E5 G5 C6 = E5 G5 F5 A5 D6 = B5 = G5 B5 C6 = = = = = = -", "R=F=R=F=", "k...s...k.k.s...", false, false);
	}

	// ------------------------------------------------ Secuenciador de música

	static int Nota(string s) {
		string[] nombres = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
		string n = s.Substring(0, s.Length - 1).Replace("Db", "C#").Replace("Eb", "D#").Replace("Gb", "F#").Replace("Ab", "G#").Replace("Bb", "A#");
		int oct = s[s.Length - 1] - '0';
		return 12 * (oct + 1) + Array.IndexOf(nombres, n);
	}

	//Devuelve la nota base (octava 2) y si el acorde es menor
	static (int raiz, bool menor) Acorde(string a) {
		bool menor = a.EndsWith("m");
		string n = menor ? a.Substring(0, a.Length - 1) : a;
		return (Nota(n + "2"), menor);
	}

	//bpm, acordes por compás, melodía en corcheas ("-" silencio, "=" mantener),
	//patrón de bajo de 8 corcheas (R raíz, T tercera, F quinta, O octava), batería de 16 semicorcheas
	static AudioClip Secuenciar(string nombre, float bpm, string[] acordes, string melodia, string bajo, string bateria,
		bool arpegio, bool bucle, float eco = 0f, float cicloMelodia = 0.25f) {

		float semicorchea = 60f / bpm / 4f;
		int compases = acordes.Length;
		int pasos = compases * 16;
		int largo = (int)(pasos * semicorchea * FRECUENCIA);
		int cola = bucle ? 0 : (int)(1.2f * FRECUENCIA);
		var buf = new float[largo + cola];
		var r = new System.Random(nombre.GetHashCode());

		void Sumar(int i, float v) {
			if (bucle) buf[((i % largo) + largo) % largo] += v;
			else if (i < buf.Length) buf[i] += v;
		}

		//Instrumento con envolvente simple
		void Tocar(float inicio, float duracion, float freq, Func<float, float> onda, float volumen, float decaimiento, bool vibrato) {
			int a = (int)(inicio * FRECUENCIA);
			int n = (int)((duracion + 0.04f) * FRECUENCIA);
			float faseLocal = 0f;
			for (int k = 0; k < n; k++) {
				float t = k / (float)FRECUENCIA;
				float env = Mathf.Min(1f, t / 0.006f) * Mathf.Lerp(1f, 0.55f, Mathf.Min(1f, t * decaimiento));
				if (t > duracion) env *= 1f - (t - duracion) / 0.04f;
				float f = freq * (vibrato && t > 0.15f ? 1f + 0.006f * Seno(5.5f, t) : 1f);
				faseLocal += f / FRECUENCIA;
				Sumar(a + k, onda(faseLocal) * env * volumen);
			}
		}

		//Melodía
		var tokens = melodia.Split(new[] { ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < tokens.Length; i++) {
			if (tokens[i] == "-" || tokens[i] == "=") continue;
			int dur = 1;
			while (i + dur < tokens.Length && tokens[i + dur] == "=") dur++;
			float ini = i * 2 * semicorchea;
			Tocar(ini, dur * 2 * semicorchea * 0.92f, Midi(Nota(tokens[i])), ph => Pulso(ph, cicloMelodia), 0.22f, 3f, true);
		}

		for (int c = 0; c < compases; c++) {
			var (raiz, menor) = Acorde(acordes[c]);
			int tercera = menor ? 3 : 4;
			float inicioCompas = c * 16 * semicorchea;

			//Bajo
			if (!string.IsNullOrEmpty(bajo)) {
				for (int p = 0; p < 8; p++) {
					char ch = bajo[p % bajo.Length];
					if (ch == '-' || ch == '=') continue;
					int dur = 1;
					while (p + dur < 8 && bajo[(p + dur) % bajo.Length] == '=') dur++;
					int nota = raiz + (ch == 'F' ? 7 : ch == 'O' ? 12 : ch == 'T' ? tercera : 0);
					Tocar(inicioCompas + p * 2 * semicorchea, dur * 2 * semicorchea * 0.85f, Midi(nota), ph => Triangulo(ph), 0.32f, 2f, false);
				}
			}

			//Arpegio suave en semicorcheas
			if (arpegio) {
				int[] intervalos = { 0, tercera, 7, 12, 7, tercera };
				for (int p = 0; p < 16; p++) {
					int nota = raiz + 36 + intervalos[p % intervalos.Length];
					Tocar(inicioCompas + p * semicorchea, semicorchea * 0.6f, Midi(nota), ph => Pulso(ph, 0.125f), 0.05f, 8f, false);
				}
			}

			//Batería
			if (!string.IsNullOrEmpty(bateria)) {
				for (int p = 0; p < 16; p++) {
					char ch = bateria[p % bateria.Length];
					int a = (int)((inicioCompas + p * semicorchea) * FRECUENCIA);
					if (ch == 'k') {
						for (int k = 0; k < 0.15f * FRECUENCIA; k++) {
							float t = k / (float)FRECUENCIA;
							Sumar(a + k, Seno(Mathf.Lerp(140f, 45f, t / 0.15f), t) * Mathf.Exp(-t * 22f) * 0.55f);
						}
					} else if (ch == 's') {
						for (int k = 0; k < 0.16f * FRECUENCIA; k++) {
							float t = k / (float)FRECUENCIA;
							Sumar(a + k, (Ruido(r) * 0.8f + Seno(190f, t) * 0.3f) * Mathf.Exp(-t * 20f) * 0.3f);
						}
					} else if (ch == 'h' || ch == 'g') {
						float largoH = ch == 'g' ? 0.09f : 0.035f;
						float prev = 0f;
						for (int k = 0; k < largoH * FRECUENCIA; k++) {
							float t = k / (float)FRECUENCIA;
							float n = Ruido(r);
							float hp = n - prev;
							prev = n;
							Sumar(a + k, hp * Mathf.Exp(-t * (ch == 'g' ? 30f : 70f)) * 0.09f);
						}
					}
				}
			}
		}

		//Eco (para la cueva)
		if (eco > 0f) {
			int retraso = (int)(semicorchea * 3 * FRECUENCIA);
			var seco = (float[])buf.Clone();
			for (int i = 0; i < buf.Length; i++) {
				int j = i - retraso;
				if (bucle) j = ((j % buf.Length) + buf.Length) % buf.Length;
				if (j >= 0) buf[i] += seco[j] * eco;
			}
		}

		return GuardarClip(nombre, buf, bucle);
	}

	static void CrearMusica(Juego j) {

		mMenu = Secuenciar("Musica_Menu", 118f, new[] { "C", "Am", "F", "G", "C", "Am", "F", "G" },
			"E5 = G5 = C6 = G5 = | A5 = = G5 E5 = C5 = | F5 = A5 = C6 = A5 G5 | G5 = = = D5 = - - | " +
			"E5 G5 C6 = B5 C6 D6 = | C6 = A5 = E5 = A5 = | F5 = E5 F5 A5 = G5 F5 | E5 = D5 = G5 = = -",
			"R=-RF=-F", "k.h.s.h.k.h.s.h.", true, true);

		//Nivel 1: merengue playero
		mNivel1 = Secuenciar("Musica_Nivel1", 150f, new[] { "G", "G", "C", "D", "G", "Em", "C", "D" },
			"G5 = B5 = D6 = B5 G5 | A5 B5 A5 G5 E5 = D5 = | E5 = G5 = C6 = B5 A5 | F#5 = A5 = D6 = = - | " +
			"G5 = B5 D6 E6 D6 B5 G5 | E5 G5 B5 = A5 G5 E5 = | C5 E5 G5 C6 B5 A5 G5 E5 | D5 = F#5 = A5 = G5 =",
			"R=-FR=-F", "k.ggs.gsk.ggs.gs", true, true);

		//Nivel 2: cueva misteriosa con eco
		mNivel2 = Secuenciar("Musica_Nivel2", 104f, new[] { "Am", "Am", "F", "G", "Am", "Em", "F", "E" },
			"A4 = = C5 E5 = = = | D5 = C5 = B4 = = - | C5 = = A4 F5 = = = | E5 = D5 = B4 = = - | " +
			"A5 = = G5 E5 = = C5 | B4 = = = G4 = B4 = | C5 = A4 = F5 = E5 D5 | E5 = = = G#4 = = -",
			"R===F===", "k.......h...s...", true, true, 0.35f, 0.5f);

		//Nivel 3: noche en la loma
		mNivel3 = Secuenciar("Musica_Nivel3", 128f, new[] { "Dm", "Dm", "A#", "C", "Dm", "Gm", "A", "A" },
			"D5 = F5 = A5 = G5 F5 | E5 = D5 = A4 = = - | Bb4 = D5 = F5 = D5 = | C5 = E5 = G5 = E5 C5 | " +
			"D5 F5 A5 = D6 = C6 A5 | Bb5 = A5 G5 D5 = G5 = | A5 = = E5 C#5 = E5 = | A5 = G5 = E5 = C#5 =",
			"R=RFR=RO", "k.h.s.h.k.k.s.h.", true, true);

		//Jefe: música distinta, rápida e intensa
		mJefe = Secuenciar("Musica_Jefe", 168f, new[] { "Em", "Em", "C", "D", "Em", "Em", "B", "B" },
			"E5 E5 G5 E5 B5 = A5 G5 | F#5 G5 F#5 E5 D5 = E5 = | C6 = B5 = G5 = E5 G5 | A5 = F#5 = D5 = F#5 A5 | " +
			"B5 = E6 = D6 B5 G5 B5 | A5 G5 F#5 G5 E5 = = - | D#5 = F#5 = B5 = A5 F#5 | D#6 = B5 = F#5 = D#5 =",
			"RORORORO", "k.hhs.hhk.k.s.hs", true, true, 0f, 0.5f);

		mVictoria = Secuenciar("Musica_Victoria", 132f, new[] { "C", "F", "G", "C" },
			"C5 E5 G5 C6 = = G5 C6 | A5 = F5 A5 C6 = A5 = | B5 = G5 B5 D6 = B5 G5 | C6 = = = G5 = C6 =",
			"R=RFR=RO", "k.h.s.h.k.h.s.hh", true, true);

		j.musicaMenu = mMenu;
		j.musicaVictoria = mVictoria;
	}
}
