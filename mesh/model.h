#pragma once

#include "../shaders/shader.h"
#include "./mesh.h"

#include <vector>
#include <string>

#include <assimp/Importer.hpp>
#include <assimp/scene.h>
#include <assimp/postprocess.h>

#include <stb_image.h>


unsigned int TextureFromFile(const char* path, const std::string& directory);

class Model
{
public:
	Model(std::string const &path)
	{
		loadModel(path);
	}
	void Draw(Shader& shader);
private:
	//model data
	std::vector<Mesh> meshes;
	std::string directory;
	std::vector<Texture> textures_loaded;

	void loadModel(std::string const &path);
	void processNode(aiNode* node, const aiScene* scene);
	Mesh processMesh(aiMesh* mesh, const aiScene* scene);
	std::vector<Texture> loadMaterialTextures(aiMaterial* mat, aiTextureType type, std::string typeName);
};