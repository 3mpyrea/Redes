using Fusion;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public class HandView : MonoBehaviour
{

    [SerializeField] List<CardView> hand = new List<CardView>();
    public IEnumerator AddToHand(List<CardView> source, int totalCardsToAdd)
    {
        int remainingCards = totalCardsToAdd;
        if (remainingCards <= 0) yield break;

        
        // Variables de diseño del abanico
        float separationX = 140f;
        float archHeight = 45f;
        float maxRotation = 15f;
        Vector2 center = new Vector2(0f, -400f);

        int safetyIterator = 0;
        int maxSafetyLoops = 20;

        while (remainingCards > 0 && safetyIterator < maxSafetyLoops)
        {
            safetyIterator++;

            // 2. VERIFICACIÓN DE SEGURIDAD DE LA CARTA
            if (source == null || source.Count == 0)
            {
                yield return null;
                continue;
            }

            CardView cardToDraw = source[0];
            if (cardToDraw == null)
            {
                source.RemoveAt(0);
                remainingCards--;
                continue;
            }

            // Activamos la carta que vamos a robar
            cardToDraw.gameObject.SetActive(true);

            // LÓGICA DE JUEGO INMEDIATA: Añadimos la carta a la mano de datos antes de animar
            hand.Add(cardToDraw);
            source.RemoveAt(0);

            remainingCards--;
            //GameManager.instance.drawTxt.gameObject.GetComponent<TxtManager>().TextChange(source.Count);

            // --- REORGANIZACIÓN TOTAL Y SIMÉTRICA DEL ABANICO ---
            // Obtenemos cuántas cartas hay en TOTAL en la mano en este milisegundo (viejas + la nueva)
            int totalCartasEnMano = hand.Count;

            for (int indexMano = 0; indexMano < totalCartasEnMano; indexMano++)
            {
                CardView cartaActual = hand[indexMano];
                if (cartaActual == null) continue;

                RectTransform rectCarta = cartaActual.GetComponent<RectTransform>();

                float posX = 0f;
                float posY = 0f;
                float dirZ = 0f;

                if (totalCartasEnMano > 1)
                {
                    // La matemática ahora se calcula basándose en la posición real de cada carta dentro de la mano completa (indexMano)
                    float t = (float)indexMano / (totalCartasEnMano - 1) * 2f - 1f;
                    float proportionalHandWidth = (totalCartasEnMano - 1) * separationX;

                    posX = t * (proportionalHandWidth / 2f);
                    posY = (1f - (t * t)) * archHeight;
                    dirZ = -t * maxRotation;
                }

                Vector2 targetAnchoredPos = center + new Vector2(posX, posY);
                Vector3 targetRotation = new Vector3(0f, 0f, dirZ);

                // Animamos de forma asíncrona tanto las cartas viejas para que se abran, 
                // como la carta nueva para que tome su posición correcta en el abanico.
                rectCarta.DOAnchorPos(targetAnchoredPos, 0.4f);
                rectCarta.DOLocalRotate(targetRotation, 0.4f);
            }

            // Pausa entre el robo de cada carta (reducido a 0.4s para que se sienta dinámico, cámbialo si prefieres 1f)
            yield return new WaitForSeconds(0.4f);
        }

        Debug.Log("Robo finalizado exitosamente.");
    }
}
