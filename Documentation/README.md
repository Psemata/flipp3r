# README
Ce fichier regroupe les informations importantes du projet et du programme Flipp3r.
L'arborescence du projet, où sont situées les vidéos de démonstration, les logiciels utilisés ainsi qu'où se trouve la procédure d'installation.

## Arborescence
Flipp3r
| aruco_detection
    | arucos -- Dossier contenant tous les différents arucos
    | test_images -- Différentes images utilisées pour les tests de calibrage
    | calibrationAruco.py  -- Script python utilisé pour le calibrage
    | matrix.txt -- Matrice de calibrage sauvgardée
| Démonstrations -- Dossier contenant toutes les vidéos de démonstrations du projet enregistrées au fil du travail
| Documentation
    | bc-flipper-report -- Dossier regroupant les fichiers pour le rapport LaTeX
    | Calibrage - Rapport - Traitement d'image -- Rapport du projet de traitement d'image sur le calibrage du projet
    | Réunion - Modèle -- Modèle Word Office pour les suivis de réunion
    | Réunion Flipp3r - 100% - Organisation -- Powerpoint de réunion pour le début du 100% du projet
| Flipp3r
    | Assets
        | Animations -- Dossier contenant toutes les animations
        | Audio -- Dossier contenant tous les sons et toutes les musiques du jeu
        | Font -- Dossier contenant la police d'écriture du projet
        | HDRPDefaultRessources -- Ressources pour HDRP
        | Materials -- Les matériaux du projet
        | Models -- Les modèles 3D du projet
        | Prefabs -- Les différentes prefabs du projet
        | Scenes -- Les scènes du projet
        | Scripts -- Les scripts du jeu
        | Shaders -- Les différents shaders
        | TextMesh Pro -- Ressources pour TextMesh Pro
        | Textures -- Les textures du projet
        | VFX -- Les VFX du projet
        | Videos -- Les vidéos du boss du jeu
    | Packages -- Les packages du projet Unity
    | ProjectSettings -- Les paramètres du projet Unity
    | .gitignore -- Git Ignore du projet
| .gitattributes -- Attributs utilisés pour Git LFS
| .gitlab-ci.yml -- Fichier utilisés pour le CICD gitlab
| README.md -- Fichie readme du projet

### Vidéos de démonstration
Toutes les vidéos de démonstration des étapes du projet sont disponibles dans le sous-dossier "Démonstrations" du dossier "Flipp3r".
La vidéo de démonstration finale se trouve à la racine de l'arborescence.

## Logiciels utilisés
Les logiciels principaux utilisés sont :
- Unity - 2021.3.2f1
- Visual Studio Code

Le reste des outils et éléments de workflow sont détaillés sur la page correspondante du wiki du projet Flipp3r

## Procédure d'installation
La procédure d'înstallation est précisée dans le fichier "Installation et utilisation.md" disponible au même endroit que ce README.md