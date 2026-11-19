# Ranking de la comunidad

🌐 [English](community-ranking.md) | [Português](community-ranking.pt.md) | **Español**

El ranking de la comunidad es un segundo ranking, construido a partir de lo que los jugadores piensan unos de otros. El sitio le muestra dos jugadores a un votante y le pregunta cuál es mejor. Cada respuesta sube o baja a los dos jugadores. No se reinicia con la temporada.

[Volver al manual](README.es.md)

## Contenido

- [La página del ranking de la comunidad](#la-página-del-ranking-de-la-comunidad)
- [Votar](#votar)
- [Quién puede votar, y cuánto](#quién-puede-votar-y-cuánto)
- [Cómo se elige la pareja](#cómo-se-elige-la-pareja)
- [Cómo se calcula la puntuación](#cómo-se-calcula-la-puntuación)
- [Para administradores](#para-administradores)

## La página del ranking de la comunidad

**Comunidad** en el menú abre la lista.

![El ranking de la comunidad](../images/es/community.webp)

| Columna | Significado |
|---|---|
| # | Posición. Los tres primeros tienen una medalla. |
| Patente | La patente del jugador en la temporada actual del ranking de estadísticas. |
| Jugador | Avatar y nombre. |
| Puntuación | La puntuación construida a partir de los votos. Todos empiezan con 1500. |
| Comparaciones | Cuántas veces se juzgó al jugador frente a otro jugador. |
| Victorias, Empates, Derrotas | Cuántas comparaciones ganó, empató y perdió el jugador. |

Solo aparecen los jugadores con suficientes comparaciones: 5 de forma predeterminada. El mínimo está escrito debajo de la tabla. **Votar** abre la página de votación.

## Votar

![La página de votación](../images/es/community-vote.webp)

La página muestra dos jugadores con sus avatares. Responde con una de estas opciones:

| Respuesta | Qué hace |
|---|---|
| Este es mejor (izquierda o derecha) | El jugador elegido gana la comparación y el otro la pierde. |
| Están al mismo nivel | Un empate. |
| No conozco a este jugador (debajo de uno de ellos) | No cambia nada, y ese jugador no se te vuelve a mostrar. |
| No conozco a ninguno de los dos | No cambia nada, y ninguno de los dos se te vuelve a mostrar. |

Después de cada respuesta la página marca tu elección y entra la siguiente pareja. En el ordenador, las flechas ← y → eligen al jugador de la izquierda o de la derecha. **Votos restantes hoy** muestra cuántos votos te quedan.

## Quién puede votar, y cuánto

| Regla | Predeterminado |
|---|---|
| Tienes que haber iniciado sesión con Steam. | |
| Tienes que haber jugado un número mínimo de partidas, contadas en todas las temporadas, del ranking o no. | 10 |
| Tienes un número de votos por día. El día sigue la zona horaria del sitio. Solo las respuestas que juzgan (mejor o mismo nivel) gastan un voto; "No lo conozco" no. | 20 |
| Un jugador se ofrece para comparar solo después de un número mínimo de partidas. | 1 |
| Nunca se te pregunta por ti mismo, ni dos veces por la misma pareja. | |

Cuando no puedes votar, la página dice por qué: tienes que iniciar sesión, necesitas más partidas (con tu cantidad), usaste todos los votos del día o no queda nadie para que compares.

## Cómo se elige la pareja

El sitio elige la pareja; un votante no puede elegir a quién juzgar, y una respuesta solo se acepta para la pareja que ofreció el sitio.

1. Prefiere jugadores con los que o contra los que jugaste, porque son los que mejor conoces. Solo cuando no queda ninguna pareja así ofrece otros jugadores.
2. Entre ellos, parte de uno de los jugadores con menos comparaciones, para que los jugadores nuevos reciban una puntuación.
3. Empareja a ese jugador con alguien de puntuación cercana.
4. Cuál aparece a la izquierda es aleatorio.

## Cómo se calcula la puntuación

La puntuación usa el sistema Elo, la misma idea que en el ajedrez.

- Todos los jugadores empiezan con la puntuación inicial, 1500 de forma predeterminada.
- Antes de una comparación, el sitio calcula la probabilidad de que cada jugador sea juzgado mejor, a partir de la diferencia entre sus puntuaciones. Una diferencia de 400 puntos significa más o menos 10 a 1.
- El ganador gana y el perdedor pierde la misma cantidad de puntos: el factor K (32 de forma predeterminada) multiplicado por la diferencia entre el resultado (1 por una victoria, ½ por un empate, 0 por una derrota) y la probabilidad esperada.
- Ganarle a un jugador con una puntuación mucho más alta da muchos puntos; ganarle a uno mucho más débil da pocos. Un empate entre jugadores de puntuaciones distintas sube al más débil.
- Las respuestas "No lo conozco" no cambian nada.

La puntuación es permanente. No vuelve a empezar con la temporada.

## Para administradores

**Administración > Ranking de la comunidad** tiene los ajustes, el mantenimiento y la lista de votantes.

![Los ajustes del ranking de la comunidad](../images/es/admin-community.webp)

| Ajuste | Qué hace | Predeterminado |
|---|---|---|
| Mínimo de partidas para votar | Partidas que un jugador tiene que haber jugado, en todas las temporadas, para votar. | 10 |
| Votos por día | Respuestas que juzgan que cada votante puede dar por día. Como mínimo 1. | 20 |
| Mínimo de partidas para ser comparado | Partidas que un jugador tiene que haber jugado para que se ofrezca para comparar. | 1 |
| Mínimo de comparaciones para aparecer en la lista | Comparaciones que un jugador necesita para aparecer en el ranking de la comunidad. | 5 |
| Factor K de Elo | Cuánto mueve las puntuaciones un voto, de 1 a 200. | 32 |
| Puntuación inicial | La puntuación de un jugador que nunca fue comparado. | 1500 |

El factor K y la puntuación inicial se aplican a los votos nuevos. **Reconstruir las puntuaciones** vuelve a aplicar en orden todos los votos guardados con los ajustes actuales, para que se apliquen a todos los votos.

**Votantes** lista a todos los que votaron, con su cantidad de votos y su último voto. **Quitar votos** elimina todos los votos de un votante y reconstruye las puntuaciones sin ellos, por ejemplo cuando alguien votó de mala fe.

Desactivar el módulo del ranking de la comunidad en [Módulos](administration.es.md#módulos) quita **Comunidad** del menú. Los votos se conservan.
