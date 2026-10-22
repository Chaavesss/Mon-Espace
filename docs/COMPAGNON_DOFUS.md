# Compagnon Dofus — version 1.7

## Démarrer

1. Ouvrir **Tous mes outils → Compagnon Dofus** dans l’espace souhaité, par exemple Gaming.
2. Cliquer sur **+ Personnage** et renseigner son nom, sa classe, son alignement et son ordre si nécessaires. Le serveur est facultatif. Les choix du guide ne sont pas devinés à partir du nom ou du jeu.
3. Choisir **RSA · Rush Sylvestre Accéléré** ou **Dofus Sylvestre**, puis **Charger ce guide**. Le bouton **Charger le catalogue Tougli** donne accès aux autres parcours publics.
4. Utiliser **Terminé →** pour valider et avancer, **Suivant** pour naviguer sans valider, **Précédent** pour revenir. **Annuler la validation** permet de corriger une coche.
5. Cliquer sur **Vue compacte** pour garder l’étape au premier plan pendant le jeu.

Le suivi est propre au personnage, au parcours et à l’espace actif de Mon espace. Changer de personnage ou de guide conserve les autres suivis. Le redémarrage retrouve la sélection et la dernière étape enregistrée. Deux personnages de même nom peuvent être distingués par leur serveur. Les profils sont uniquement locaux et ne créent pas de personnages dans Tougli.

## Les contenus affichés

Le lecteur utilise le catalogue et les données publiques du site Tougli : https://tougli.barbofus.com/guides . Les deux parcours proposés au départ sont :

- RSA – Rush Sylvestre Accéléré : https://tougli.barbofus.com/guide/rsa-rush-sylvestre-accelere
- Dofus Sylvestre : https://tougli.barbofus.com/guide/dofus-sylvestre

Les parcours donnent un ordre de quêtes, des notes et des informations de préparation. Le compagnon affiche les formats observés : sections, notes, quêtes, groupes, succès, quêtes à prendre/avancer et choix de personnage. Les choix de classe, d’alignement et d’ordre suivent le profil renseigné. Un choix non renseigné ou un format non pris en charge reste signalé et ne peut pas être validé comme s’il avait été interprété. Il faut alors compléter le profil ou consulter la source.

Les détails de quête reprennent les prérequis, contraintes et quantités fournis par Tougli. Ils ne remplacent pas toutes les pages détaillées des quêtes. **Guide détaillé** ouvre le lien fourni par Tougli dans le navigateur par défaut, par exemple Opera GX. Aucun navigateur intégré ni Edge n’est ajouté.

Les coordonnées et **Copier /travel** sont proposés uniquement quand le texte reçu contient des coordonnées explicites au format [x,y]. Le compagnon ne connaît pas la position du personnage. La commande est copiée seulement sur clic, jamais envoyée au jeu.

**Ressources restantes** additionne les quantités des quêtes encore présentes dans les étapes non validées ; les répétitions d’une même quête ne sont pas comptées plusieurs fois. L’inventaire, les achats et les ressources déjà consommées ne sont pas connus. Ce total est une préparation indicative issue du guide, pas un calcul de stock personnel.

L’avancement est celui des étapes du guide, y compris ses notes et rappels ; ce n’est pas une vérification automatique de vos succès en jeu. Valider une étape est une action manuelle.

## Fenêtre compacte

La fenêtre affiche le personnage, le parcours, l’étape actuelle, ses indications et les premières ressources de la quête. Les ressources longues et l’ensemble des étapes restent disponibles dans la page principale. La progression est commune aux deux vues.

Déplacer la fenêtre par son titre, redimensionner par son bord et choisir **Toujours au premier plan**. La taille et la position sont enregistrées à la fermeture. Le mode discret masque les noms, le texte et la progression. La fenêtre reste indépendante de la fenêtre principale ; elle n’injecte rien dans Dofus. Le fonctionnement au-dessus du jeu dépend de son mode d’affichage et doit être vérifié sur le PC cible.

Le widget général propose aussi **Compagnon Dofus** lorsqu’un personnage existe dans cet espace.

## Mode de lancement Dofus

Dans l’éditeur d’un mode, **Compagnon Dofus à la fin du lancement** permet de choisir : Automatique, Ne pas ouvrir, Page, Vue compacte ou les deux.

Automatique ouvre la page et la vue compacte pour un mode ayant l’accueil Dofus ou portant exactement le nom Dofus ; il ne modifie pas les autres modes. Une destination explicite vers un projet ou une note est conservée. Un lancement depuis une autre activité qui demande de rester sur sa page conserve également cette page et peut afficher la vue compacte.

Les ouvertures d’Ankama Launcher, Spotify et autres applications restent celles déjà configurées sur le PC. Si le mode ouvre aussi Tougli dans le navigateur, retirer cette ancienne action si elle fait doublon avec le compagnon. Le mode ne télécharge pas automatiquement un guide absent : le premier chargement se fait depuis la page du compagnon.

## Données, cache et sauvegardes

Les validations et la position dans le parcours sont enregistrées dans les données de Mon espace. Elles sont incluses dans les exports et sauvegardes complètes. Le cache des guides publics est séparé et n’est pas inclus dans ces sauvegardes : après restauration sur un autre PC, recharger le même parcours pour retrouver ses étapes. Les identifiants d’étapes encore identiques servent à retrouver les validations.

**Actualiser ce guide** recharge son contenu à la demande. Une erreur réseau, un format invalide ou une réponse correspondant à un autre parcours ne remplace pas le dernier guide valide. Les contenus peuvent évoluer ; les validations anciennes ne sont pas attribuées à d’autres étapes. Une mention prévient si des validations ne correspondent plus aux étapes ou aux choix actuels.

La lecture hors ligne nécessite que le guide ait déjà été chargé sur ce PC. Le catalogue complet se recharge en ligne. Le paquet publié contient seulement le lecteur et des tests fictifs, aucun catalogue complet ni guide Tougli préchargé.

Source et auteurs : **Tougli — Toufloux et Gligo**. Cette intégration est indépendante de Tougli et Ganymède et dépend des interfaces publiques actuellement accessibles. Aucun compte, cookie ou jeton Tougli n’est utilisé. Les coches existantes sur Tougli/Ganymède ne sont pas importées ni synchronisées.

## Validation du développement

Les tests automatiques couvrent le format des données, les branches de classe et les choix manquants, les noms de prérequis, les ressources répétées, les étapes manuelles et leur annulation, l’isolation des personnages, la persistance après redémarrage, l’annulation sur échec d’écriture, les liens et le texte HTML, les sauvegardes et le mode discret. La page et la fenêtre compacte sont rendues en WPF ; les boutons réels de validation et le formulaire de personnage sont exécutés dans les tests.

Les deux guides demandés ont été lus par l’exécutable Windows avec les données publiques réelles. Le téléchargement du RSA a aussi été vérifié par le client HTTP natif. Les tests CI utilisent des données fictives et ne dépendent pas de la disponibilité de Tougli.

Pour un contrôle manuel facultatif : **MonEspace.exe --dofus-download-test DOSSIER_TEST** télécharge le RSA public et écrit un résultat technique dans ce dossier. **--dofus-reader-test DOSSIER_TEST** lit le fichier guide.json déjà présent. Ces commandes ne lancent pas le jeu et ne modifient pas l’espace utilisateur normal.

Le placement avec la souris, le redimensionnement et l’affichage au-dessus du jeu, ainsi que les ouvertures des applications configurées, restent des vérifications sur le PC cible.
