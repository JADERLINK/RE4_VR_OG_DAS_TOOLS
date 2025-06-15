# **RE4_VR_OG_DAS_TOOLS**

Tool to edit DAS files of RE4VR OG version!
<br>Use these tools only on DAS files from the RE4 VR OG version.
<br>When creating files with this tool, this file is not compatible with the DATUDAS TOOL.

**Translate from Portuguese Brazil:**

Tool destinada a extrair e reconstruir os arquivos DAS da versão de RE4 VR OG, com a possibilidade de manter os Offset Originais de alguns arquivos, especialmente os arquivo BIN e TPL, pois o jogo os offsets que apontam para os arquivo para definir a sobreposição de modelos 3D e texturas, dentro dos arquivos do UE é chamado de "OffsetKey", no qual você pode manter o original com essas tools, ou verificar os novos offsets gerados, e mudar nos arquivos do UE, caso ao contrário os modelos 3D Ficaram invisíveis.

<br>O que é um offset? Nesse caso, é um valor que indica onde está um arquivo dentro de outro arquivo, isto é, o arquivo DAS é, na verdade, um conjunto de arquivos, e o offset é um número para saber onde cada arquivo começa dentro do arquivo DAS, e o jogon no VR uso o valor do offset para saber qual é cada arquivo.
<br>Mas para acomodar todos os arquivos, caso você insira um arquivo maior, os offsets subsequentes mudam, as tools são feitas para fixar o offset de alguns arquivos (BINs e TPLs), e os outros serão colocados de maneira que não tenha sobreposição de arquivos dentro do DAS, mas tome cuidado com os offsets fixos, pois caso você fizer algo errado você pode sobrepor algum arquivo indevidamente.


## RE4_VR_OG_DAS_OFFSETKEY_TOOL.exe

Essa tool gera um arquivo txt2, que é apenas informativos, com ele você vai saber qual são os Offset Key e o tamanho de cada arquivo.
<br>Use "RE4_VR_OG_DAS_OFFSETKEY_TOOL.bat", para gerar um txt2 informativo de todos os arquivos;
<br>Use "RE4_VR_OG_DAS_OFFSETKEY_TOOL only BINs.bat" para saber o offset somente dos arquivos BINS.
<br>Nota: você pode editar esse bat e adicionar mais arquivos, por exemplo: $BIN:TPL:EFF no lugar de $BIN

## RE4_VR_OG_NEWDAS_TOOL.exe

Ao extrair ele vai criar uma pasta com os arquivos, e um arquivo "IDXRE4VRDAS", esse arquivo é parecido com o do DATUDAS_TOOL, porem além do nome do arquivo, é informado também o Offset Key, caso for 0, o offset vai ser escolhido pelo programa, por padrão é definido o offset key para os arquivo BIN/TPL/EFF;
<br>Nota: edite a partir de arquivos DAS originais do jogo, arquivos já modificados do UHD não vão funcionar, pois vão ter offsets diferentes dos originais.
<br>Sobre o arquivo "FormatsToShowOffsets.txt", esse arquivo define quais formatos vai ter o offset key ao extrair.
<br> Nota2: ao fazer o repack, os arquivos serão colocados no arquivo DAS, em uma ordem arbitraria, então não use o DATUDAS_TOOL para extrair o arquivo DAS, somente essa tool novamente.

## RE4_VR_OG_INSERTDAS_TOOL.exe

Esta tool em vez de extrair e recriar um novo DAS, ele coloca os arquivos dentro de um DAS existente.
<br> Primeiro, você deve passar o DAS sobre a tool para criar um arquivo "INSERTDASRE4VR" no qual será usado para a tool inserir os arquivos.
<br> Você deve abrir e editar o arquivo "INSERTDASRE4VR". Para que um arquivo seja realmente inserido, você deve apagar o formato que está dentro de parenteses () e o texto vai começar com "DAT_";
<br> Nota: O arquivo em questão também deve existir conforme especificado dentro do "INSERTDASRE4VR";


**At.te: JADERLINK**
<br>2025-06-15