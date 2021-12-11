#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;
out vec2 texCoord;

void main()
{   
    FragPos = vec3(mat4(1.0f) * vec4(aPosition, 1.0));
    texCoord = aTexCoord;
    gl_Position = projection * view * vec4(FragPos, 1.0);
}