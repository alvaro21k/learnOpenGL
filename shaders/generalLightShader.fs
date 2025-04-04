#version 330 core

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};

struct DirLight
{
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight
{
    vec3 position;
    float constant;
    float linear;
    float quadratic;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
#define NR_POINT_LIGHTS 4  

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float constant;
    float linear;
    float quadratic;
};

uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform DirLight dirLight;
uniform SpotLight spotLight;

in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPos;

uniform Material material;
uniform vec3 viewPos;

out vec4 FragColor;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir); 
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir); 

void main()
{
    //properties
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    // Directional lightning
    vec3 result = CalcDirLight(dirLight, norm, viewDir);

    //Point lights
    for (int i = 0; i < NR_POINT_LIGHTS; i++)
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
        
    // Spotlight
    result += CalcSpotLight(spotLight, norm, FragPos, viewDir);

    FragColor = vec4(result,1.0);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    //diffuse shading
    float diff = max(dot(normal, lightDir),0.0);

    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    //calculate components and combine them
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;

    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);

    //diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir,reflectDir),0.0),material.shininess);

    float distance =    length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    //calculate components and combine them
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;

    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);

    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff)/epsilon,0.0,1.0);

    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;

    vec3 norm = normalize(normal);

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir,reflectDir),0.0),material.shininess);
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;

    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance)); 

    diffuse *= attenuation;
    specular *= attenuation;

    diffuse *= intensity;
    specular *= intensity;

    return (ambient + diffuse + specular);
}