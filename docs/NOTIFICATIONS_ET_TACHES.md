# Notifications et tâches liées — version 1.5

## Centre de notifications

Cliquer sur la cloche en haut à droite. Le compteur indique les alertes à lire, hors reports en cours. **À lire** affiche ce qui demande une attention ; **Toutes les actives** permet de retrouver les alertes déjà lues ou reportées.

- Tâches non terminées : dès leur rappel, ou dès le jour de l’échéance sans rappel.
- Rendez-vous locaux : du rappel à la fin du rendez-vous. Sans rappel, dès le début.
- Budget manuel : solde négatif, découvert autorisé dépassé ou prévu, enveloppe mensuelle atteinte, opérations à venir selon les réglages de rappels budget.
- Incidents d’enregistrement et d’export complet ; nouvelle version détectée.

**Ouvrir** retrouve l’élément. **Marquer comme lu** masque l’alerte de la liste à lire. **Reporter** la remet à lire après 15 minutes, 1 heure ou 24 heures. Une alerte résolue disparaît même si elle était reportée. Les incidents peuvent être retirés. Les changements sont conservés après redémarrage. En cas d’échec d’écriture, l’incident peut rester seulement en mémoire jusqu’à un enregistrement réussi.

Le mode discret masque la cloche et les informations personnelles. Les réglages de notifications Windows n’empêchent pas de consulter ce centre. Les rappels demandent que l’application reste ouverte ou réduite ; aucun service Windows n’est ajouté. Le compteur se rafraîchit au plus tard au prochain passage du minuteur, environ 15 secondes. Les 100 derniers incidents et 1 000 états de lecture/report sont conservés. Les longues listes affichent 200 alertes à la fois.

Les notifications des événements Google et du bot Discord ne sont pas connectées à ce centre dans cette version.

## Prérequis entre tâches

Dans l’éditeur d’une tâche, rechercher puis cocher les tâches à terminer dans **Attendre la fin de…**. La recherche porte sur tous les projets ; les sélections restent conservées en changeant la recherche. Maximum 100 prérequis par tâche.

La carte affiche **En attente** avec les titres concernés. Une tâche bloquée peut rester à faire, mais ne peut pas passer en cours, être terminée ou démarrer une session de concentration. La validation refuse les dépendances sur soi-même et les boucles. Les suggestions automatiques du jour ignorent les tâches bloquées.

Terminer les prérequis débloque la tâche automatiquement. Si un prérequis terminé est rouvert, le blocage réapparaît pour les transitions futures : une tâche déjà terminée n’est pas rouverte automatiquement. Supprimer un prérequis désactive son blocage ; le restaurer rétablit son lien. La prochaine occurrence d’une tâche récurrente conserve les mêmes prérequis, sans recréer une série de prérequis.

Les données et réglages restent propres à chaque utilisateur. Aucun compte partagé n’est ajouté.
