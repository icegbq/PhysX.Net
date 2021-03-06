#include "StdAfx.h"
#include "ConstraintInfo.h"

ConstraintInfo^ ConstraintInfo::ToManaged(PxConstraintInfo* info)
{
	auto ci = gcnew ConstraintInfo();
	
	ci->Constraint = ObjectTable::TryGetObject<PhysX::Constraint^>((intptr_t)info->constraint);
	ci->TypeId = info->type;
	ci->ExternalReference = ObjectTable::TryGetObject<Object^>((intptr_t)info->externalReference);

	return ci;
}