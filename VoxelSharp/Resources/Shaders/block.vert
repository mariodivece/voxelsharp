#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in mat4 aInstanceMatrix;

uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;
out vec2 TexCoords;

void main()
{
    gl_Position = vec4(aPos, 1.0) * aInstanceMatrix * view * projection;
    FragPos = vec3(vec4(aPos, 1.0) * aInstanceMatrix);
    Normal = aNormal * mat3(transpose(inverse(aInstanceMatrix)));
    TexCoords = aTexCoords;
}
