# Mon espace

Application Windows personnelle : accueil, tâches, notes, projets, planning, budget manuel, raccourcis, modes de lancement, musique et widget.

**Windows 10/11 x64 · .NET Framework 4.8 · version 1.10.0.0**

## Télécharger et installer

1. Ouvrir [la dernière version](https://github.com/Chaavesss/Mon-Espace/releases/latest).
2. Dans **Assets**, télécharger **MonEspace.zip**. Les archives **Source code** proposées par GitHub servent au développement.
3. Extraire le ZIP, quitter l’ancienne instance depuis l’icône près de l’horloge, puis lancer **MonEspace.exe**.
4. Suivre la fenêtre d’installation. Le mode **Mode_portable.cmd** permet de lancer l’application sans copier son exécutable.

[Instructions d’installation](docs/INSTALLATION.txt) · [Guide complet des mises à jour](docs/MISES_A_JOUR.md)

## Mises à jour

Dans **Préférences → Mises à jour**, le canal par défaut est :

https://github.com/Chaavesss/Mon-Espace/releases/latest/download/version.json

Sur une ancienne version, saisir cette adresse si elle est vide. L’application vérifie au démarrage si cette option est active, propose les nouvelles versions et contrôle le SHA-256 du fichier téléchargé. **L’installation reste manuelle** : extraire le nouveau ZIP et lancer son exécutable.

## Données personnelles

Les données restent sous **%LOCALAPPDATA%\MonEspace**, avec les données de travail dans **Data**. Chaque compte Windows et chaque PC disposent de leurs propres données et connexions. Aucun compte Google, Spotify ou bot Discord n’est fourni dans ce dépôt. Aucun MP3 n’est distribué.

Avant une mise à jour importante, utiliser la sauvegarde intégrée. Une mise à jour conserve le dossier de données ; les exécutables s’installent dans **Versions/<version>**. Ne pas supprimer **Data** pour mettre à jour.

## Développement

Le dépôt contient les sources C#, les icônes, le script de compilation et la chaîne de publication GitHub Actions.

Dans Windows PowerShell, depuis la racine du dépôt :

    .\build.ps1
    .\scripts\test.ps1
    .\scripts\prepare-release.ps1 -SkipBuild

Le compilateur .NET Framework, les bibliothèques WPF/WinForms et les métadonnées WinRT de Windows sont nécessaires. Le script utilise les composants Windows installés. Résultat local : **release/MonEspace.exe**, puis **dist/MonEspace.zip** et **dist/version.json**.

Le workflow **Compiler et publier Mon espace** compile, vérifie les suites compatibles avec Windows Server, prépare le paquet et publie une nouvelle version si son numéro n’existe pas encore. Le test PDF natif complet est exécuté sur Windows desktop : **desktop-validation.json** lie cette validation aux empreintes des sources et scripts. La publication est bloquée si cette validation manque ou ne correspond plus au code. Voir [le guide](docs/MISES_A_JOUR.md) pour les versions suivantes et les reprises après un échec.

## État et limites

- PDF, images et documents texte s’ouvrent dans l’application. Les fichiers Google Docs/Sheets sont exportés en PDF en lecture seule ; ce n’est pas un éditeur Sheets complet.
- Google Drive et Calendar demandent leur configuration OAuth et l’activation des deux API. Les scénarios API ont été vérifiés avec des réponses simulées ; chaque connexion réelle doit encore être validée avec son compte.
- Les fonctions Spotify dépendent du compte, de la configuration API et d’un appareil de lecture disponible.
- L’exécutable n’est pas signé par un certificat éditeur. L’installation sur le PC où un accès refusé avait été rencontré reste à vérifier.
- Les contrôles de publication ne suppriment aucune donnée locale. Le SHA-256 vérifie l’intégrité du téléchargement ; il ne remplace pas une signature éditeur.

Les marques et logos des services tiers appartiennent à leurs titulaires. Aucune licence de redistribution du code n’est accordée par défaut dans ce dépôt.

Le moteur PDF natif présente un problème de fermeture sur les serveurs GitHub Windows Server 2022/2025 après un rendu réussi. Ce point est documenté dans le guide. Le test PDF complet reste actif par défaut sur Windows desktop.

## Nouveautés 1.4

Trois priorités choisies pour la journée, reprise des notes/projets/documents récents, réception des captures accessible depuis l’accueil, déplacement et redimensionnement des blocs, et accueils associés aux modes Travail/Dofus/Perso. [Guide d’utilisation](docs/MA_JOURNEE.md).

## Nouveautés 1.5

Cloche de notifications, reports et suivi de lecture ; prérequis entre tâches et protection contre les boucles ; installation des mises à jour et relance depuis l’application. Voir [le guide](docs/NOTIFICATIONS_ET_TACHES.md) et [la procédure de mise à jour](docs/MISES_A_JOUR.md).

## Nouveautés 1.6

Planning hebdomadaire, rappels en une phrase, espaces indépendants, historique des notes et sauvegardes complètes automatiques. [Mode d’emploi](docs/PLANNING_ESPACES_SAUVEGARDES.md).

## Nouveautés 1.7

Compagnon Dofus avec parcours Tougli à la demande, RSA / Sylvestre, progression locale par personnage, ressources et fenêtre compacte. [Guide d’utilisation](docs/COMPAGNON_DOFUS.md).
