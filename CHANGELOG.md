# [1.0.0-alpha.4](https://github.com/georgepharrison/FlowRight/compare/v1.0.0-alpha.3...v1.0.0-alpha.4) (2025-09-01)


### Features

* implement TASK-047 RFC 7807 ValidationProblemDetails support ([#40](https://github.com/georgepharrison/FlowRight/issues/40)) ([d19b794](https://github.com/georgepharrison/FlowRight/commit/d19b794709e05f6b758caf8ec224ab144ba3b229))

# [1.0.0-alpha.3](https://github.com/georgepharrison/FlowRight/compare/v1.0.0-alpha.2...v1.0.0-alpha.3) (2025-09-01)


### Features

* implement explicit 2xx HTTP status code mapping for TASK-046 ([#39](https://github.com/georgepharrison/FlowRight/issues/39)) ([f7397ab](https://github.com/georgepharrison/FlowRight/commit/f7397ab781b9394df335847f0a2bb35d6f67a100))

# [1.0.0-alpha.2](https://github.com/georgepharrison/FlowRight/compare/v1.0.0-alpha.1...v1.0.0-alpha.2) (2025-09-01)


### Features

* complete TASK-045 comprehensive content type handling ([#38](https://github.com/georgepharrison/FlowRight/issues/38)) ([8202dc4](https://github.com/georgepharrison/FlowRight/commit/8202dc421011c7d2bb6e2dbf04d109cb8715ef53))

# 1.0.0-alpha.1 (2025-09-01)


### Bug Fixes

* update GitHub Actions workflows to use .NET 9.0 ([#37](https://github.com/georgepharrison/FlowRight/issues/37)) ([e22b383](https://github.com/georgepharrison/FlowRight/commit/e22b3837bf943d320a6678922b2f1c1b0fd1ffea))


### Features

* add implicit and explicit operators for seamless conversions (TASK-013) ([#7](https://github.com/georgepharrison/FlowRight/issues/7)) ([4feb0ab](https://github.com/georgepharrison/FlowRight/commit/4feb0abeaa00c2d1ad3e03a67a2cd6490aeb528a))
* complete TASK-008 Result<T> generic type support ([2421953](https://github.com/georgepharrison/FlowRight/commit/2421953d11c0dd7617861ddf0e9a0ac63385be36))
* complete TASK-019 ValidationProblemResponse serialization support ([#13](https://github.com/georgepharrison/FlowRight/issues/13)) ([cd26a1e](https://github.com/georgepharrison/FlowRight/commit/cd26a1ef8ba85111297819b1087e6396e6511323))
* complete TASK-020 comprehensive round-trip serialization testing ([#14](https://github.com/georgepharrison/FlowRight/issues/14)) ([000a348](https://github.com/georgepharrison/FlowRight/commit/000a348e9ba734039fc3ef311693dd26ae2171a9))
* complete TASK-021 ValidationBuilder<T> class with comprehensive testing ([#15](https://github.com/georgepharrison/FlowRight/issues/15)) ([aff5dac](https://github.com/georgepharrison/FlowRight/commit/aff5dacb4392699023e99bb24c36baff0b0c7b21))
* complete TASK-022 PropertyValidator base class implementation ([#16](https://github.com/georgepharrison/FlowRight/issues/16)) ([9a3ffa5](https://github.com/georgepharrison/FlowRight/commit/9a3ffa50141c834b70f71cacd88e16eb36453d5c))
* complete TASK-023 - add RuleFor methods for all property types ([#17](https://github.com/georgepharrison/FlowRight/issues/17)) ([2e67ab7](https://github.com/georgepharrison/FlowRight/commit/2e67ab7d40a9ed46b0cdb5f42b76ae5684d80fa3))
* complete TASK-024 error aggregation system ([#18](https://github.com/georgepharrison/FlowRight/issues/18)) ([8190dca](https://github.com/georgepharrison/FlowRight/commit/8190dcaa8e4f8da05fb656d575b04613ec1051fa))
* complete TASK-025 Build method with factory pattern ([#19](https://github.com/georgepharrison/FlowRight/issues/19)) ([8302e4b](https://github.com/georgepharrison/FlowRight/commit/8302e4b44ce70257dd131c9f6f98b266430a3859))
* complete TASK-027 NumericPropertyValidator for all numeric types ([#21](https://github.com/georgepharrison/FlowRight/issues/21)) ([03d93b8](https://github.com/georgepharrison/FlowRight/commit/03d93b8fadba1c783e0367f40dcdf183715412a4))
* complete TASK-031 - implement additional string validation rules ([#24](https://github.com/georgepharrison/FlowRight/issues/24)) ([296706d](https://github.com/georgepharrison/FlowRight/commit/296706d0a8a324c412b7dce2a1d64e64d4c00672))
* complete TASK-035 conditional validation rules (When, Unless) ([#27](https://github.com/georgepharrison/FlowRight/issues/27)) ([0b5c409](https://github.com/georgepharrison/FlowRight/commit/0b5c409e7afe5ac725a1024c112c5690f1b1c94d))
* complete TASK-036 RuleFor Result<T> composition ([#28](https://github.com/georgepharrison/FlowRight/issues/28)) ([5a18151](https://github.com/georgepharrison/FlowRight/commit/5a1815123aac5e28bb6dac709757108aff881f8d))
* complete TASK-037 automatic error extraction from nested Results ([#29](https://github.com/georgepharrison/FlowRight/issues/29)) ([6ae0cb3](https://github.com/georgepharrison/FlowRight/commit/6ae0cb3042809e2cf2ee89b25cf3f5960738a38b))
* complete TASK-038 out parameter support for value extraction ([#30](https://github.com/georgepharrison/FlowRight/issues/30)) ([12eeb52](https://github.com/georgepharrison/FlowRight/commit/12eeb527a91cda7e026b7f8a7656efc9945a5056))
* complete TASK-039 add validation context for complex scenarios ([#31](https://github.com/georgepharrison/FlowRight/issues/31)) ([a407b29](https://github.com/georgepharrison/FlowRight/commit/a407b2921d3377799fbd821b3d09b004b53618d5))
* complete TASK-040 custom message support with WithMessage ([#32](https://github.com/georgepharrison/FlowRight/issues/32)) ([665eccc](https://github.com/georgepharrison/FlowRight/commit/665ecccb9b2ee5be3fe4cdc616cf3b2fafb398a0))
* complete TASK-042 with comprehensive HttpResponseMessageExtensions tests ([#34](https://github.com/georgepharrison/FlowRight/issues/34)) ([53ecc85](https://github.com/georgepharrison/FlowRight/commit/53ecc85bf565c66e14996d81fef18c388021531f))
* complete TASK-043 with null-safe JSON result conversion ([fdc7d2d](https://github.com/georgepharrison/FlowRight/commit/fdc7d2da3eb647fc6366a739014a02a31d45f008))
* implement async-friendly extension methods (TASK-015) ([#9](https://github.com/georgepharrison/FlowRight/issues/9)) ([830b9a2](https://github.com/georgepharrison/FlowRight/commit/830b9a2abb6c81676f0d3cbcac788ca283e44d70))
* implement Combine method for result aggregation (TASK-010) ([#4](https://github.com/georgepharrison/FlowRight/issues/4)) ([294bf7c](https://github.com/georgepharrison/FlowRight/commit/294bf7c1ed96860d1ef8c412122d03926ab9ce8b))
* implement custom JsonConverter for Result class (TASK-017) ([#11](https://github.com/georgepharrison/FlowRight/issues/11)) ([73d0deb](https://github.com/georgepharrison/FlowRight/commit/73d0deb7567704d0df627f01c2718081b3e8c1bb))
* implement custom JsonConverter for Result<T> (TASK-018) ([#12](https://github.com/georgepharrison/FlowRight/issues/12)) ([1733d27](https://github.com/georgepharrison/FlowRight/commit/1733d2794d2ff4052d35eb9c7072da2c60ae0099))
* implement GuidPropertyValidator for unique identifiers (TASK-029) ([#22](https://github.com/georgepharrison/FlowRight/issues/22)) ([bfe6008](https://github.com/georgepharrison/FlowRight/commit/bfe6008a7d64d2cb286f541331ac9072604ac4f1))
* implement IResult and IResult<T> interfaces (TASK-006) ([#2](https://github.com/georgepharrison/FlowRight/issues/2)) ([c7d7103](https://github.com/georgepharrison/FlowRight/commit/c7d71038bb6029d4e41f6061293b4667c0663e68))
* implement Match method overloads for non-generic Result class (TASK-011) ([#5](https://github.com/georgepharrison/FlowRight/issues/5)) ([3327561](https://github.com/georgepharrison/FlowRight/commit/3327561b8582bdb14e81222cd63e1e0b1754471c))
* implement Switch method overloads for non-generic Result class (TASK-012) ([#6](https://github.com/georgepharrison/FlowRight/issues/6)) ([2f6349e](https://github.com/georgepharrison/FlowRight/commit/2f6349e6d180aa9aad4ec36d33954d50233299d8))
* initial commit with README and LICENSE ([b1fdbd4](https://github.com/georgepharrison/FlowRight/commit/b1fdbd48ead597d16b6d6279bc0db902b5ebdcf5))
* setup semantic versioning with GitHub Actions ([54cf3bc](https://github.com/georgepharrison/FlowRight/commit/54cf3bc869359e67a0f4e7aef6148a7ccd3706fc))
* upgrade to .NET 9.0 and C# 13.0 ([eb0a43b](https://github.com/georgepharrison/FlowRight/commit/eb0a43bc3cfe49eb25c3eea7d4c097216914a637))
