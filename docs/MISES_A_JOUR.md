# Publier et distribuer Mon espace

## 1. Ce qui est configuré

- Dépôt public : https://github.com/Chaavesss/Mon-Espace
- Branche de travail et de publication : **main**.
- Workflow : **Actions → Compiler et publier Mon espace**.
- Paquet installé : **MonEspace.zip**.
- Informations de version : **version.json**.
- Adresse permanente du canal : https://github.com/Chaavesss/Mon-Espace/releases/latest/download/version.json

Le canal devient utilisable dès la première release publiée avec ses deux fichiers. Un dépôt contenant seulement du code ne suffit pas.

## 2. Première publication

Le premier envoi sur main déclenche le workflow. Ouvrir l’onglet **Actions**, puis l’exécution la plus récente. Il faut attendre la fin des étapes **Compiler**, **Vérifier les 12 suites** (le volet PDF natif est contrôlé sur desktop, voir la section 8), **Préparer le ZIP** et **Publier une nouvelle version**.

Le workflow utilise le jeton temporaire **GITHUB_TOKEN** fourni par GitHub, avec **contents: write** uniquement pour ce job. Aucun jeton personnel à copier dans le code. Si les règles du compte bloquent Actions ou les écritures, consulter **Settings → Actions → General** et les messages de l’exécution. Ne pas ajouter de secret au hasard.

Après succès, ouvrir **Releases → Mon espace v1.3.0.0**. Les fichiers attendus dans **Assets** sont :

| Fichier | Utilité |
| --- | --- |
| MonEspace.zip | Application compilée et instructions |
| version.json | Version, adresse précise du ZIP et empreinte SHA-256 |
| Source code (zip/tar.gz) | Archives du code créées par GitHub |

Télécharger **MonEspace.zip** pour installer. Ne pas prendre **Source code**.

La publication est créée en brouillon avec ses fichiers, puis rendue publique. En cas d’échec pendant cette phase, un brouillon peut rester dans Releases ; il faut le terminer ou le supprimer avant de relancer la même version. Ne pas écraser silencieusement une version déjà distribuée.

## 3. Configurer ton PC et celui de ton frère

1. Installer la version téléchargée.
2. Ouvrir **Préférences → Mises à jour**.
3. Vérifier l’adresse du canal donnée plus haut et l’enregistrer si nécessaire.
4. Activer **Vérifier au démarrage** si souhaité.
5. Cliquer sur **Rechercher une mise à jour**.

Si la version installée est déjà la dernière, le message correspondant est normal. Il faut une version supérieure pour déclencher une proposition de téléchargement.

Les anciens réglages avec un champ de canal vide restent vides tant que l’adresse n’est pas enregistrée. Le canal est prérempli pour les nouvelles configurations de cette version. Chacun conserve son nom, ses couleurs, ses tâches, son budget, ses raccourcis et ses connexions. Le module bot reste désactivé par défaut et peut être activé uniquement sur ton PC.

**Il n’est plus nécessaire de renvoyer le dossier à ton frère à chaque changement.** Il peut télécharger la version proposée dans son application. Il devra ensuite extraire le ZIP et lancer MonEspace.exe ; aucune installation silencieuse n’est effectuée.

## 4. Publier les versions suivantes

À chaque version destinée aux utilisateurs :

1. Modifier les sources.
2. Dans **src/ReleaseInfo.cs**, augmenter **AssemblyVersion** et **AssemblyFileVersion**, par exemple de **1.3.0.0** à **1.3.1.0**. Garder les deux valeurs identiques.
3. Remplacer le contenu de **RELEASE_NOTES.md** par les changements de cette version, sans données personnelles. Maximum 10 000 caractères.
4. Exécuter **scripts/test.ps1** sur Windows 10/11 pour lancer les 12 suites complètes, y compris le PDF natif. Ajouter le fichier **desktop-validation.json** généré au même commit que les sources, puis envoyer ce commit sur **main**.
5. Suivre l’exécution dans **Actions**. Les tests doivent réussir avant publication.
6. Vérifier les deux fichiers dans la nouvelle release.

Le dossier d’installation et le manifeste utilisent automatiquement le numéro réel de l’assembly compilé. Il n’y a aucun numéro d’installation supplémentaire à mettre à jour.

Si le numéro a déjà été publié, le workflow vérifie le code mais ne remplace pas la release existante. Pour distribuer une correction, augmenter le numéro. Ne pas publier une version plus petite que la dernière : les applications installées comparent les numéros et proposeront seulement une version supérieure.

Pour relancer sans changement de code : **Actions → Compiler et publier Mon espace → Run workflow → main**. Un échec ne met pas à disposition un ZIP non vérifié.

## 5. Préparation manuelle de secours

Sur Windows 10/11 x64 avec .NET Framework 4.8, ouvrir Windows PowerShell à la racine du dépôt :

    .\build.ps1
    .\scripts\test.ps1
    .\scripts\prepare-release.ps1 -SkipBuild

Les scripts ne modifient aucune stratégie de sécurité Windows. Si la machine interdit leur exécution, utiliser GitHub Actions ou faire adapter la procédure à la politique de cette machine.

Les résultats sont dans **dist**. Dans GitHub, ouvrir **Releases → Draft a new release** :

1. Choisir/créer le tag exact indiqué dans **dist/tag.txt**, par exemple **v1.3.1.0**.
2. Cibler le commit qui a servi à compiler.
3. Coller les notes de version.
4. Joindre **dist/MonEspace.zip** et **dist/version.json**, sans les renommer.
5. Publier comme version stable et la définir comme dernière version.

Ne pas modifier le ZIP après génération du manifeste : son empreinte ne correspondrait plus. Le script peut être relancé pour recalculer l’ensemble avant publication.

## 6. Vérifications et diagnostic

| Symptôme | Vérification |
| --- | --- |
| Adresse du canal : 404 | La release doit être publiée, stable, marquée comme dernière et contenir version.json |
| Application déjà à jour | Comparer la version affichée avec celle du manifeste |
| Téléchargement refusé | Vérifier que le ZIP n’a pas changé depuis la génération du SHA-256 |
| Actions en rouge | Ouvrir l’étape en erreur ; aucun succès ne doit être supposé |
| Mise à jour téléchargée mais ancienne app visible | Quitter l’instance depuis la zone de notification, extraire et lancer le nouvel EXE |
| Accès refusé à l’installation | Consulter le diagnostic indiqué par l’application ; ne pas supprimer Data |
| Google ne se connecte pas | Vérifier OAuth, comptes test, API Drive et Calendar activées, puis reconnecter si les autorisations ont changé |

Le manifeste référence un ZIP attaché à une **version précise**, afin que son empreinte reste cohérente même si une nouvelle release arrive pendant un téléchargement. Seul le manifeste utilise le lien permanent vers **latest**.

## 7. Ce qui ne doit jamais être ajouté au dépôt

**Data**, sauvegardes personnelles, connexions Google/Spotify, fichiers OAuth privés, jetons Discord, fichiers .env, MP3, clés privées et certificats privés. Le .gitignore exclut les emplacements courants mais il ne faut pas copier ces données ailleurs dans les sources. Les données utilisateur ne sont pas nécessaires pour compiler ou publier.

## 8. Validation de cette préparation

Compilation Windows x64 réussie et 12 suites réussies sur la machine de préparation. Contrôles supplémentaires : installation dans un dossier versionné, copie vérifiée, conservation d’un fichier de données, contenu du ZIP limité aux trois fichiers attendus, manifeste lié au bon numéro et SHA-256 calculé sur le ZIP final. Sur les runners Windows Server 2022 et 2025, les assertions de rendu PDF réussissent, mais la fermeture du processus échoue dans ntdll.dll avec le code 0xc000000d. Le workflow utilise donc explicitement **-SkipNativePdf** et conserve tous les autres contrôles de la suite. Le script local sans cette option conserve le test PDF intégral. Le résultat desktop complet est enregistré dans **desktop-validation.json**, avec une empreinte des sources, icônes, compilation et scripts ; **prepare-release.ps1** refuse de publier si ces fichiers ont changé depuis la validation. Les fins de processus en erreur ne sont jamais transformées en succès. Ce dépôt ne prétend pas prendre en charge Windows Server pour un usage normal de l’application.

Références officielles : [liens permanents vers les assets](https://docs.github.com/en/repositories/releasing-projects-on-github/linking-to-releases), [création d’une release avec gh](https://cli.github.com/manual/gh_release_create).
