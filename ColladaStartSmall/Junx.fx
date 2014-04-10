#define	PI_OVER_FOUR			0.7853981634f
#define	PI_OVER_TWO				1.5707963268f

float4x4	mWorld;
float4x4	mView;
float4x4	mProjection;
float4x4	mLightViewProj;	//for shadowing
float4		mSolidColour;
float4		mSpecColor;
float4		mLightColor0;		//trilights need 3 colors
float4		mLightColor1;		//trilights need 3 colors
float4		mLightColor2;		//trilights need 3 colors
float3		mLightDirection;
float3		mEyePos;
float		mSpecPower;


struct VPos
{
	float4	Position	: POSITION;
};
struct VPosPS
{
	float4	Position	: SV_POSITION;
};
struct VPosTex03Tex13
{
	float4	Position	: POSITION;
	float3	TexCoord0	: TEXCOORD0;
	float3	TexCoord1	: TEXCOORD1;
};
struct VPosNormTex0
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
	float2	TexCoord0	: TEXCOORD0;
};
struct VVPosTex03Tex13
{
	float4	Position	: SV_POSITION;
	float4	TexCoord0	: TEXCOORD0;
	float4	TexCoord1	: TEXCOORD1;
};


//compute the 3 light effects on the vert
//see http://home.comcast.net/~tom_forsyth/blog.wiki.html
float3 ComputeTrilight(float3 normal, float3 lightDir, float3 c0, float3 c1, float3 c2)
{
    float3	totalLight;
	float	LdotN	=dot(normal, lightDir);
	
	//trilight
	totalLight	=(c0 * max(0, LdotN))
		+ (c1 * (1 - abs(LdotN)))
		+ (c2 * max(0, -LdotN));
		
	return	totalLight;
}

float3 ComputeGoodSpecular(float3 wpos, float3 lightDir, float3 pnorm, float3 lightVal, float4 fillLight)
{
	float3	eyeVec	=normalize(mEyePos - wpos);
	float3	halfVec	=normalize(eyeVec + lightDir);
	float	ndotv	=saturate(dot(eyeVec, pnorm));
	float	ndoth	=saturate(dot(halfVec, pnorm));

	float	normalizationTerm	=(mSpecPower + 2.0f) / 8.0f;
	float	blinnPhong			=pow(ndoth, mSpecPower);
	float	specTerm			=normalizationTerm * blinnPhong;
	
	//fresnel stuff
	float	base		=1.0f - dot(halfVec, lightDir);
	float	exponential	=pow(base, 5.0f);
	float	fresTerm	=mSpecColor + (1.0f - mSpecColor) * exponential;

	//vis stuff
	float	alpha	=1.0f / (sqrt(PI_OVER_FOUR * mSpecPower + PI_OVER_TWO));
	float	visTerm	=(lightVal * (1.0f - alpha) + alpha) *
				(ndotv * (1.0f - alpha) + alpha);

	visTerm	=1.0f / visTerm;

	float3	specular	=specTerm * lightVal * fresTerm * visTerm * fillLight;

	return	specular;
}


//worldpos and worldnormal
VVPosTex03Tex13 WNormWPosVS(VPosNormTex0 input)
//VPosTex03Tex13 WNormWPosVS(VPos input)
//VPosPS WNormWPosVS(VPos input)
{
	VVPosTex03Tex13	output;
//	VPosPS	output	=(VPos)0;
	
	//generate the world-view-proj matrix
	float4x4	wvp	=mul(mul(mWorld, mView), mProjection);
	
	//transform the input position to the output
	output.Position		=mul(input.Position, wvp);
	output.TexCoord0	=mul(input.Normal, mWorld);
	output.TexCoord1	=mul(input.Position, mWorld);

//	float3	norm	=-normalize(output.TexCoord1);
//	output.TexCoord0	=norm;

	//return the output structure
	return	output;
}

//Solid color, trilight, and expensive specular
float4 TriSolidSpecPhysPS(VVPosTex03Tex13 input) : SV_Target
{
//	return	float4(1, 0, 0, 1);
	float3	pnorm	=input.TexCoord0;
	float3	wpos	=input.TexCoord1;

	pnorm	=normalize(pnorm);

	float3	triLight	=ComputeTrilight(pnorm, mLightDirection,
							mLightColor0, mLightColor1, mLightColor2);

	float3	specular	=ComputeGoodSpecular(wpos, mLightDirection, pnorm, triLight, mLightColor2);
	float3	litSolid	=mSolidColour.xyz * triLight;

	specular	=saturate(specular + litSolid);

	return	float4(specular, mSolidColour.w);
}
//Solid color, trilight, and expensive specular
//float4 TriSolidSpecPhysPS(VPosPS input) : SV_Target
//{
//	return	float4(1, 0, 0, 1);
//}


technique10 SolidSpecPhys
{
	pass P0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0_level_9_3, WNormWPosVS()));
		SetPixelShader(CompileShader(ps_4_0_level_9_3, TriSolidSpecPhysPS()));
	}
}

struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
};
struct VS_IN2
{
	float4 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};
struct PS_IN2
{
	float4 pos : SV_POSITION;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	//generate the world-view-proj matrix
	float4x4	wvp	=mul(mul(mWorld, mView), mProjection);

	output.pos = mul(input.pos, wvp);
	output.col = input.col;
	
	return output;
}
PS_IN2 VS2( VS_IN2 input )
{
	PS_IN2 output = (PS_IN2)0;
	
	//generate the world-view-proj matrix
	float4x4	wvp	=mul(mul(mWorld, mView), mProjection);

	output.pos = mul(input.pos, wvp);
//	output.col = input.col;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
//	return	float4(1, 0, 0, 1);
	return input.col;
}
float4 PS2( PS_IN2 input ) : SV_Target
{
	return	float4(1, 1, 0, 1);
}

technique10 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0_level_9_3, VS() ) );
		SetPixelShader( CompileShader( ps_4_0_level_9_3, PS() ) );
	}
}

technique10 Render2
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0_level_9_3, VS2() ) );
		SetPixelShader( CompileShader( ps_4_0_level_9_3, PS2() ) );
	}
}