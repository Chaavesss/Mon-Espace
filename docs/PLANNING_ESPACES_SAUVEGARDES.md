# Organiser son quotidien — version 1.6

## Planning semaine

Ouvrir **Planning semaine** dans la navigation. Rechercher une tâche active puis la glisser sur un jour et une heure, ou cliquer sur son titre / **Planifier une tâche**. Le dialogue permet de confirmer la date, l’heure et la durée. Cliquer sur un créneau pour le modifier ou le retirer du planning. Déplacer un créneau prépare sa nouvelle position dans le dialogue avant enregistrement.

Les déplacements se calent sur 15 minutes. Un créneau dure de 15 minutes à 12 heures, sans dépasser minuit. Une tâche peut avoir plusieurs créneaux. La planification ne modifie ni son échéance ni son statut. Une tâche terminée reste visible dans ses créneaux ; elle ne compte plus dans la charge à faire. Les créneaux d’une tâche supprimée sont masqués et réapparaissent si elle est restaurée.

Les rendez-vous locaux et les événements Google déjà chargés sont affichés à côté des tâches. Un chevauchement produit un avertissement mais reste autorisé. Les éléments simultanés sont disposés côte à côte ; leur détail est disponible au survol et au clic. Les événements Google sur toute la journée figurent dans l’en-tête du jour. Le planning s’ouvre sur la plage 06h–minuit ; le bouton **Afficher les 24 heures** donne accès à la nuit.

**Capacité quotidienne** règle le temps disponible de 1 à 24 heures. La charge additionne les créneaux de tâches actives et les rendez-vous horaires : elle peut dépasser la durée réelle occupée en cas de chevauchement. Une journée dépassant la capacité est signalée. Les rendez-vous à cheval sur minuit comptent dans chaque journée concernée.

**Actualiser Google** charge la période de l’agenda sélectionné. Seuls les événements déjà reçus sont connus ; un calendrier non chargé ne permet pas de garantir qu’un créneau est libre. Les modifications de rendez-vous Google restent dans Agenda. Aucune tâche n’est envoyée à Google par le planning.

## Rappels en une phrase

Dans **Mes tâches**, cliquer sur **Rappel en une phrase** ; le même outil est accessible depuis la capture rapide.

Exemples :

- Appeler demain à 14h30
- Sport chaque lundi à 18h
- Payer le loyer le 5 de chaque mois
- Revoir le dossier après-demain
- Préparer le rendez-vous dans 3 jours
- Faire le point chaque semaine
- Déposer le dossier le 25/10/2026 à 10:00

Une prévisualisation montre le titre, la première date, l’heure et la répétition reconnue. Vérifier puis cliquer sur **Créer le rappel**. Sans heure, le rappel est fixé à 09:00. Une date explicite dans le passé reste affichée comme telle : la vérifier avant création. Une phrase non reconnue n’est pas créée. Il s’agit de formulations de date prises en charge, pas d’un assistant qui comprend toutes les phrases.

Les répétitions utilisent le fonctionnement existant : terminer une occurrence crée la suivante. Un rendez-vous ne doit donc pas être saisi ici si l’on veut un événement d’agenda. Le rappel Windows et le centre de notifications suivent les réglages habituels.

## Espaces indépendants

Le sélecteur en haut ouvre **Mes espaces**. Trois espaces sont proposés : **Perso**, **Travail**, **Gaming**. Les données d’avant la mise à jour restent dans Perso, sans déplacement. Les autres commencent vides et se configurent au premier lancement. On peut créer et renommer jusqu’à 20 espaces.

Chaque espace possède ses tâches, notes, projets, fichiers joints, finances, modes, raccourcis, couleurs, widgets, historique et connexions Google/Spotify. Les sauvegardes concernent un espace à la fois. La liste des espaces et le choix du dernier espace sont propres à ce compte Windows et ne sont pas inclus dans une sauvegarde complète individuelle.

Changer d’espace enregistre les données et demande de régler les documents non enregistrés, ferme Mon espace puis relance la même version sur l’espace choisi. Une session de concentration, une copie, une mise à jour ou une sauvegarde en cours bloque ce changement. Les applications externes déjà lancées restent ouvertes. Seul l’espace ouvert émet des rappels et effectue ses sauvegardes automatiques. Le démarrage suivant retrouve le dernier espace choisi. Les modes de lancement continuent à organiser l’accueil et ouvrir des outils à l’intérieur d’un espace.

## Historique des notes

Ouvrir **Tous mes outils → Historique des notes**, ou **Historique** depuis l’éditeur d’une note. Une version précédente est ajoutée lors d’un enregistrement qui change son titre, son texte, son projet, son passage à la corbeille ou sa suppression. Les modifications des cases à cocher et les notes du panneau latéral sont également couvertes.

Consulter une version puis **Restaurer cette version** remet son texte dans la note. La version actuelle est conservée à son tour pour pouvoir revenir en arrière. Une note supprimée peut être recréée depuis son historique. Les modifications qui n’ont jamais été enregistrées ne sont pas récupérables par ce mécanisme.

Limites : 15 versions par note, 100 versions au total et 2 Mo sérialisés. Les versions les plus anciennes sont retirées lorsque ces limites sont dépassées. Les sauvegardes JSON et complètes incluent l’historique. Restaurer une sauvegarde complète restaure son propre historique.

## Sauvegardes automatiques complètes

Dans **Réglages → Configurer les sauvegardes automatiques**, choisir un dossier privé, une fréquence et le nombre de copies, puis activer et enregistrer. Le même écran est dans **Tous mes outils**. Aucun dossier n’est choisi à votre place. Les fréquences proposées sont 1 h, 6 h, 12 h, 24 h et une semaine ; conserver de 1 à 30 copies. **Sauvegarder maintenant** déclenche une copie sans attendre l’échéance.

Une copie comprend les données enregistrées de cet espace, ses réglages, le planning, l’historique, les pièces jointes et son MP3. Les dossiers épinglés, fichiers externes, documents non enregistrés et connexions Google/Spotify ne sont pas copiés. Le fichier .mespace se restaure avec **Restaurer une sauvegarde complète**.

La copie s’effectue en arrière-plan sur un instantané des données. Les pièces jointes doivent rester disponibles pendant la copie ; une erreur de lecture annule la sauvegarde. Une copie de fichiers en cours retarde le lancement. La fermeture complète attend la fin d’une sauvegarde ; réduire ou masquer l’application reste possible. Quitter l’application arrête cette fonction : aucun service n’est installé. Une sauvegarde en retard se lance à la prochaine ouverture de cet espace. Après échec, les nouvelles tentatives automatiques sont espacées d’au moins 15 minutes.

L’écran affiche la dernière réussite et l’erreur éventuelle ; le centre de notifications signale les échecs. La rotation supprime uniquement les fichiers portant l’identifiant propre de cette configuration dans le dossier choisi. Elle ne supprime pas les sauvegardes manuelles ni les autres fichiers. La copie la plus récente est toujours conservée. Changer de dossier ne nettoie pas l’ancien dossier. Le format n’est pas chiffré : une copie sur un autre disque protège mieux contre la panne du disque principal.

## Validation

Les tests couvrent dates et récurrences, créneaux et déplacements via le dialogue, chevauchements et charge, historique et restauration, isolation des données des espaces, rotation et restauration de sauvegardes. Les nouveaux écrans sont rendus en WPF. Les glisser-déposer avec une souris physique et la relance réelle lors du changement d’espace restent à vérifier sur le PC cible. La connexion à un vrai compte Google/Spotify n’est pas exécutée par ces tests.
